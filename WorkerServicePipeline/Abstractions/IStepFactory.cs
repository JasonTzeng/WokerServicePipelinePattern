namespace WorkerServicePipeline.Abstractions
{
    public interface IStepFactory
    {
        IStep? CreateStep(string stepName, IServiceProvider serviceProvider);
        IStep? CreateStep<T>() where T : IStep;
        IEnumerable<IStep> CreateSteps();
    }
}
