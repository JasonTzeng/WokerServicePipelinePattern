using Confluent.Kafka;
using WorkerServicePipeline.Abstractions;

namespace WorkerServicePipeline.Messaging
{
    public class KafkaConsumer : IEventConsumer
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly IConfiguration _config;

        public KafkaConsumer(ILogger<KafkaConsumer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task ConsumeAsync(string topic, Func<string, Task> onMessage, CancellationToken cancellationToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:Consumer:BootstrapServers"],
                GroupId = _config["Kafka:Consumer:GroupId"],
                AutoOffsetReset = Enum.TryParse<AutoOffsetReset>(_config["Kafka:Consumer:AutoOffsetReset"], true, out var offset) ? offset : AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(topic);

            try
            {
                var result = consumer.Consume(cancellationToken);
                if (result != null)
                {
                    _logger.LogInformation("Message received from topic {Topic} with key {Key} and value {Value}", topic, result.Message.Key, result.Message.Value);
                    await onMessage(result.Message.Value);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer cancelled.");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
