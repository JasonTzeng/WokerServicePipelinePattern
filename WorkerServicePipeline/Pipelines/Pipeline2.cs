using WorkerServicePipeline.Abstractions;

namespace WorkerServicePipeline.Pipelines
{
    public class Pipeline2 : IPipeline
    {
        private readonly IConfiguration _config;
        private readonly IStepFactory _stepFactory;
        private readonly ILogger<Pipeline2> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IStep> _steps = new();

        public Pipeline2(
            IConfiguration config,
            IStepFactory stepFactory,
            ILogger<Pipeline2> logger,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _stepFactory = stepFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;

            var stepNames = _config.GetSection("Pipelines:Pipeline2").Get<List<string>>() ?? new();
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
