using System.Text.Json;
using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class PublishToKafka : IStep
    {
        private readonly ILogger<PublishToKafka> _logger;
        private readonly IEventPublisher _kafkaPublisher;
        private readonly IConfiguration _config;
        private readonly CaseContext _context;

        public PublishToKafka(ILogger<PublishToKafka> logger,
            IEventPublisher kafkaPublisher,
            IConfiguration config,
            CaseContext context)
        {
            _logger = logger;
            _kafkaPublisher = kafkaPublisher;
            _config = config;
            _context = context;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("PublishToKafkaExecution");
            activity?.SetTag("step", "PublishToKafka");

            _logger.LogInformation("PublishToKafka step started.");

            try
            {
                // 將 _context 轉成 JSON 字串
                string json = JsonSerializer.Serialize(_context);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("PublishToKafka step skipped due to empty serialized context.");
                    return;
                }

                string topic = _config["Kafka:Publisher:Topic"] ?? "";
                if (string.IsNullOrWhiteSpace(topic))
                {
                    _logger.LogWarning("Kafka topic is not configured.");
                    return;
                }

                await _kafkaPublisher.PublishAsync(topic, json, cancellationToken);
                _logger.LogInformation("PublishToKafka step completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishToKafka step encountered an error.");
            }
        }
    }
}
