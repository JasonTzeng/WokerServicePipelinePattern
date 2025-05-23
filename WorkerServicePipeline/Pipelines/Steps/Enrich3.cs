using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class Enrich3 : IStep
    {
        private readonly ILogger<Enrich3> _logger;
        private readonly CaseContext _context;

        public Enrich3(ILogger<Enrich3> logger,
            CaseContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Enrich3 step started.");

            try
            {
                _context.EnrichedData["Enrich3"] = "Enrich3 data";
                _logger.LogInformation("Enrich3 step completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrich3 step encountered an error.");
            }
        }
    }
}
