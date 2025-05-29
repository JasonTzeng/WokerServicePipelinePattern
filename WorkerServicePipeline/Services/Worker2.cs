using Confluent.Kafka;
using WorkerServicePipeline.Abstractions;

namespace WorkerServicePipeline.Services
{
    public class Worker2 : BackgroundService
    {
        private readonly ILogger<Worker2> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;

        public Worker2(ILogger<Worker2> logger, IServiceProvider serviceProvider, IConfiguration config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker2 started at: {time}", DateTime.Now);

            // 讀取 Pipeline 名稱
            var pipelineName = _config["Workers:Worker2"];
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                _logger.LogError("No pipeline configured for Worker2.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    // 透過名稱取得 Pipeline 實例
                    var pipelineType = Type.GetType($"WorkerServicePipeline.Pipelines.{pipelineName}");
                    if (pipelineType == null)
                    {
                        _logger.LogError("Pipeline type {PipelineName} not found.", pipelineName);
                        return;
                    }

                    var pipeline = scope.ServiceProvider.GetRequiredService(pipelineType) as IPipeline;
                    if (pipeline == null)
                    {
                        _logger.LogError("Pipeline {PipelineName} not registered.", pipelineName);
                        return;
                    }

                    await pipeline.ExecuteAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing pipeline");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Worker2 stopping at: {time}", DateTime.Now);
        }
    }
}
