namespace WorkerServicePipeline.Abstractions
{
    public interface IEventConsumer
    {
        Task ConsumeAsync(string topic, Func<string, Task> onMessage, CancellationToken cancellationToken);
    }
}
