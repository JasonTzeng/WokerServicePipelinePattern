using WorkerServicePipeline.Abstractions;

namespace WorkerServicePipeline.Pipelines
{
    public class Pipeline1 : IPipeline
    {
        private readonly IConfiguration _config;
        private readonly IStepFactory _stepFactory;
        private readonly ILogger<Pipeline1> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IStep> _steps = new();

        public Pipeline1(
            IConfiguration config,
            IStepFactory stepFactory,
            ILogger<Pipeline1> logger,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _stepFactory = stepFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;

            var stepNames = _config.GetSection("Pipelines:Pipeline1").Get<List<string>>() ?? new();
            foreach (var stepName in stepNames)
            {
                var step = _stepFactory.CreateStep(stepName, _serviceProvider);
                if (step != null)
                {
                    _steps.Add(step);
                    _logger.LogInformation("Step {StepName} created.", stepName);
                }
                else
                {
                    _logger.LogWarning("Step not found: {StepName}", stepName);
                }
            }
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            foreach (var step in _steps)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                _logger.LogInformation("Executing step {StepName}", step.GetType().Name);
                await step.ExecuteAsync(cancellationToken);
            }
        }
    }
}
