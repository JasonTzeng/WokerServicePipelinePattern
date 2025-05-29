using SolaceSystems.Solclient.Messaging;
using WorkerServicePipeline.Abstractions;
using ISession = SolaceSystems.Solclient.Messaging.ISession;

namespace WorkerServicePipeline.Messaging
{
    public class SolacePublisher : IEventPublisher, IDisposable
    {
        private readonly ILogger<SolacePublisher> _logger;
        private readonly IConfiguration _config;
        private readonly ISession _session;

        public SolacePublisher(ILogger<SolacePublisher> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var props = new SessionProperties
            {
                Host = _config["Solace:Publisher:Host"],
                VPNName = _config["Solace:Publisher:VPN"],
                UserName = _config["Solace:Publisher:Username"],
                Password = _config["Solace:Publisher:Password"]
            };

            ContextFactory.Instance.Init(new ContextFactoryProperties());
            _session = ContextFactory.Instance.CreateContext(new ContextProperties(), null)
                .CreateSession(props, null, null);
            _session.Connect();
        }

        public Task PublishAsync(string topic, string message, CancellationToken cancellationToken)
        {
            var msg = ContextFactory.Instance.CreateMessage();
            msg.Destination = ContextFactory.Instance.CreateTopic(topic);
            msg.BinaryAttachment = System.Text.Encoding.UTF8.GetBytes(message);

            _session.Send(msg);
            _logger.LogInformation("Message sent to Solace topic {Topic}", topic);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
