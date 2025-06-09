using SolaceSystems.Solclient.Messaging;
using WorkerServicePipeline.Abstractions;
using ISession = SolaceSystems.Solclient.Messaging.ISession;

namespace WorkerServicePipeline.Messaging
{
    public class SolaceConsumer : IEventConsumer, IDisposable
    {
        private readonly ILogger<SolaceConsumer> _logger;
        private readonly IConfiguration _config;
        private readonly ISession _session;
        private IFlow? _flow;
        private EventHandler<MessageEventArgs>? _topicHandler;
        private IQueue? _queue;
        private ITopic? _topic;

        public SolaceConsumer(ILogger<SolaceConsumer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            var props = new SessionProperties
            {
                Host = _config["Solace:Consumer:Host"],
                VPNName = _config["Solace:Consumer:VPN"],
                UserName = _config["Solace:Consumer:Username"],
                Password = _config["Solace:Consumer:Password"],
                // Enable topic dispatching
                TopicDispatch = true
            };

            ContextFactory.Instance.Init(new ContextFactoryProperties());
            _session = ContextFactory.Instance.CreateContext(new ContextProperties(), null)
                .CreateSession(props, null, null);
            _session.Connect();
        }

        public Task ConsumeAsync(string destinationName, Func<string, Task> onMessage, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource();

            // Determine if it is a queue or topic based on the naming convention
            if (destinationName.EndsWith("/QUEUE", StringComparison.OrdinalIgnoreCase))
            {
                // Queue (Flow-based)
                _queue = ContextFactory.Instance.CreateQueue(destinationName);

                var flowProps = new FlowProperties
                {
                    AckMode = MessageAckMode.ClientAck,
                };

                _flow = _session.CreateFlow(
                    flowProps,
                    _queue,
                    null, // Set to null if no additional subscription is needed
                    async (sender, args) =>
                    {
                        var text = System.Text.Encoding.UTF8.GetString(args.Message.BinaryAttachment);
                        _logger.LogInformation("Received message from Solace queue {Queue}", destinationName);
                        await onMessage(text);

                        // Fix: Use the IMessage extension method to acknowledge the message
                        _flow?.Ack(args.Message.ADMessageId); // Confirm the message has been processed
                        _flow?.Dispose(); // Stop after receiving a single message
                        tcs.TrySetResult(); // Complete the Task after receiving the message
                    },
                    // Update the error logging callback for the queue flow to correctly handle FlowEventArgs
                    (sender, args) => _logger.LogError("Solace queue flow error: {Error}", args.Info)
                );
                _flow.Start();
            }
            else
            {
                // Topic (Direct Message Event)
                _topic = ContextFactory.Instance.CreateTopic(destinationName);

                _topicHandler = async (sender, args) =>
                {
                    var text = System.Text.Encoding.UTF8.GetString(args.Message.BinaryAttachment);
                    _logger.LogInformation("Received message from Solace topic {Topic}", destinationName);
                    await onMessage(text);
                    tcs.TrySetResult(); // Complete the Task after receiving the message
                };

                // Fix: Use the IDispatchTarget interface to subscribe to the topic
                var dispatchTarget = _session.CreateDispatchTarget(_topic, _topicHandler);
                try
                {
                    var rc = _session.Subscribe(dispatchTarget, 0, null);
                    _logger.LogInformation("Subscribe return code: {ReturnCode}", rc);
                }
                catch (OperationErrorException ex)
                {
                    // Fix: Replace 'SubCode' with 'ErrorInfo.SubCode' to correctly access the sub-code
                    _logger.LogError(ex, "Failed to subscribe. SubCode: {SubCode}, Message: {Message}", ex.ErrorInfo.SubCode, ex.Message);
                    throw;
                }
            }

            cancellationToken.Register(() =>
            {
                Dispose();
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }

        public void Dispose()
        {
            _flow?.Dispose();

            // Unsubscribe for topic
            if (_topic is ITopic topic)
            {
                _session.Unsubscribe(topic, true);
            }

            _session?.Dispose();
        }
    }
}