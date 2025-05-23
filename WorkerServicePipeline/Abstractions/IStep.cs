namespace WorkerServicePipeline.Abstractions
{
    public interface IStep
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
