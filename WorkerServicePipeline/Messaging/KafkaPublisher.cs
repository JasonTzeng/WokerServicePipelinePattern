using Confluent.Kafka;
using WorkerServicePipeline.Abstractions;

namespace WorkerServicePipeline.Messaging
{
    public class KafkaPublisher : IEventPublisher
    {
        private readonly ILogger<KafkaPublisher> _logger;
        private readonly IConfiguration _config;
        private readonly string? _bootstrapServers;
        private readonly IProducer<string, string> _producer;

        public KafkaPublisher(ILogger<KafkaPublisher> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _bootstrapServers = _config["Kafka:Publisher:BootstrapServers"];
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers
            };
            _producer = new ProducerBuilder<string, string>(kafkaConfig).Build();
        }
        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = message
                }, cancellationToken);
                _logger.LogInformation("Message sent to topic {Topic} with key {Key} and value {Value}", topic, result.Key, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to Kafka: {Message}", ex.Message);
            }
        }
    }
}
