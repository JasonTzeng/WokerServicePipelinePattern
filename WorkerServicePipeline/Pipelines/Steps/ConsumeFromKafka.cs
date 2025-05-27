using System.Text.Json;
using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Messaging;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class ConsumeFromKafka : IStep
    {
        private readonly ILogger<ConsumeFromKafka> _logger;
        private readonly IEventConsumer _kafkaConsumer;
        private readonly IConfiguration _config;
        private readonly CaseContext _context;

        public ConsumeFromKafka(ILogger<ConsumeFromKafka> logger,
            IEventConsumer kafkaConsumer,
            IConfiguration config,
            CaseContext context)
        {
            _logger = logger;
            _kafkaConsumer = kafkaConsumer;
            _config = config;
            _context = context;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("ConsumeFromKafkaExecution");
            activity?.SetTag("step", "ConsumeFromKafka");

            _logger.LogInformation("ConsumeFromKafka step started.");

            try
            {
                string topic = _config["Kafka:Consumer:Topic"] ?? "";
                if (string.IsNullOrWhiteSpace(topic))
                {
                    _logger.LogWarning("Kafka topic is not configured.");
                    return;
                }

                await _kafkaConsumer.ConsumeAsync(topic, async message =>
                {
                    try
                    {
                        var context = JsonSerializer.Deserialize<CaseContext>(message);
                        if (context != null)
                        {
                            // 將反序列化後的內容複製到現有 _context
                            _context.RawJson = context.RawJson;
                            _context.EnrichedData = context.EnrichedData ?? new Dictionary<string, object>();
                            _logger.LogInformation("Deserialized message into CaseContext: RawJson={RawJson}, EnrichedDataCount={EnrichedDataCount}", _context.RawJson, _context.EnrichedData.Count);
                        }
                        else
                        {
                            _logger.LogWarning("Deserialized CaseContext is null.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize message to CaseContext.");
                    }

                    await Task.CompletedTask;
                }, cancellationToken);

                _logger.LogInformation("ConsumeFromKafka step completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConsumeFromKafka step encountered an error.");
            }
        }

    }
}
