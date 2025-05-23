namespace WorkerServicePipeline.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken);
    }
}
