using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class SendToKafka : IStep
    {
        private readonly ILogger<SendToKafka> _logger;
        private readonly IEventPublisher _kafkaPublisher;
        private readonly IConfiguration _config;
        private readonly CaseContext _context;

        public SendToKafka(ILogger<SendToKafka> logger,
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
            using var activity = Telemetry.ActivitySource.StartActivity("SendToKafkaExecution");
            activity?.SetTag("step", "SendToKafka");

            _logger.LogInformation("SendToKafka step started.");

            try
            {
                _context.RawJson = "All good";
                if (string.IsNullOrWhiteSpace(_context.RawJson))
                {
                    _logger.LogWarning("SendToKafka step skipped due to empty RawJson.");
                    return;
                }

                string topic = _config["Kafka:Topic"] ?? "";
                //await _kafkaPublisher.PublishAsync(topic, _context.RawJson, cancellationToken);
                _logger.LogInformation("SendToKafka step completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendToKafka step encountered an error.");
            }
        }
    }
}
