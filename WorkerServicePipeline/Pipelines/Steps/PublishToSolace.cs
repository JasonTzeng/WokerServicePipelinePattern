using System.Text.Json;
using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class PublishToSolace : IStep
    {
        private readonly ILogger<PublishToSolace> _logger;
        private readonly IEventPublisher _solacePublisher;
        private readonly IConfiguration _config;
        private readonly CaseContext _context;

        public PublishToSolace(ILogger<PublishToSolace> logger,
            [FromKeyedServices("solace")] IEventPublisher solacePublisher,
            IConfiguration config,
            CaseContext context)
        {
            _logger = logger;
            _solacePublisher = solacePublisher;
            _config = config;
            _context = context;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("PublishToSolaceExecution");
            activity?.SetTag("step", "PublishToSolace");

            _logger.LogInformation("PublishToSolace step started.");

            try
            {
                // 將 _context 轉成 JSON 字串
                string json = JsonSerializer.Serialize(_context);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("PublishToSolace step skipped due to empty serialized context.");
                    return;
                }

                string topic = _config["Solace:Publisher:Topic"] ?? "";
                if (string.IsNullOrWhiteSpace(topic))
                {
                    _logger.LogWarning("solace topic is not configured.");
                    return;
                }

                await _solacePublisher.PublishAsync(topic, json, cancellationToken);
                _logger.LogInformation("PublishToSolace step completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishToSolace step encountered an error.");
            }
        }
    }
}
