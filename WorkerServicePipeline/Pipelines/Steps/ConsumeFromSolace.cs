using System.Text.Json;
using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class ConsumeFromSolace : IStep
    {
        private readonly ILogger<ConsumeFromSolace> _logger;
        private readonly IEventConsumer _solaceConsumer;
        private readonly IConfiguration _config;
        private readonly CaseContext _context;

        public ConsumeFromSolace(ILogger<ConsumeFromSolace> logger,
            [FromKeyedServices("solace")] IEventConsumer SolaceConsumer,
            IConfiguration config,
            CaseContext context)
        {
            _logger = logger;
            _solaceConsumer = SolaceConsumer;
            _config = config;
            _context = context;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("ConsumeFromSolaceExecution");
            activity?.SetTag("step", "ConsumeFromSolace");

            _logger.LogInformation("ConsumeFromSolace step started.");

            try
            {
                //string topic = _config["Solace:Consumer:Topic"] ?? "";
                string topic = _config["Solace:Consumer:Queue"] ?? "";

                if (string.IsNullOrWhiteSpace(topic))
                {
                    _logger.LogWarning("solace topic is not configured.");
                    return;
                }

                await _solaceConsumer.ConsumeAsync(topic, async message =>
                {
                    try
                    {
                        var context = JsonSerializer.Deserialize<CaseContext>(message);
                        if (context != null)
                        {
                            // 將反序列化後的內容複製到現有 _context
                            _context.RawJson = context.RawJson;
                            _context.EnrichedData = context.EnrichedData ?? new Dictionary<string, object>();
                            _logger.LogInformation("Deserialized message into CaseContext: RawJson={RawJson}, EnrichedDataCount={EnrichedDataCount}", _context.RawJson, _context.EnrichedData.Count);
                        }
                        else
                        {
                            _logger.LogWarning("Deserialized CaseContext is null.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize message to CaseContext.");
                    }

                    await Task.CompletedTask;
                }, cancellationToken);

                _logger.LogInformation("ConsumeFromSolace step completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConsumeFromSolace step encountered an error.");
            }
        }

    }
}
