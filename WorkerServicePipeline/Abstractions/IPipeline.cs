namespace WorkerServicePipeline.Abstractions
{
    public interface IPipeline
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
