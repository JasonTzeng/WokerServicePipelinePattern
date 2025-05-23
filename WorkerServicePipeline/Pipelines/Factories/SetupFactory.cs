using WorkerServicePipeline.Abstractions;

namespace WorkerServicePipeline.Pipelines.Factories
{
    public class SetupFactory : IStepFactory
    {
        private readonly ILogger<SetupFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SetupFactory(ILogger<SetupFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public IStep? CreateStep(string stepName, IServiceProvider serviceProvider)
        {
            //using var scope = _serviceProvider.CreateScope();
            var stepType = Type.GetType($"WorkerServicePipeline.Pipelines.Steps.{stepName}");
            return stepType != null
                ? serviceProvider.GetService(stepType) as IStep
                : null;
        }

        public IStep? CreateStep<T>() where T : IStep
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IStep> CreateSteps()
        {
            throw new NotImplementedException();
        }
    }
}
