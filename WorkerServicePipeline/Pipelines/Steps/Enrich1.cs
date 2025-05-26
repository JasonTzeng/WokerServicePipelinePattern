using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Apis.Interfaces;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class Enrich1 : IStep
    {
        private readonly ILogger<Enrich1> _logger;
        private readonly IFakeApiClient _fakeApiClient;
        private readonly CaseContext _context;

        public Enrich1(ILogger<Enrich1> logger,
            IFakeApiClient fakeApiClient,
            CaseContext context)
        {
            _logger = logger;
            _fakeApiClient = fakeApiClient;
            _context = context;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("Enrich1Execution");
            activity?.SetTag("step", "Enrich1");

            _logger.LogInformation("Enrich1 step started.");

            try
            {
                var posts = (await _fakeApiClient.GetPostsAsync()).ToList();
                if (posts.Any())
                {
                    // 將所有 Post 存入 EnrichedData
                    _context.EnrichedData["Enrich1_Posts"] = posts;
                    _logger.LogInformation("Enrich1 step completed successfully. Stored {Count} posts.", posts.Count);
                }
                else
                {
                    _logger.LogWarning("Enrich1 step: No posts returned from API.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrich1 step encountered an error.");
            }
        }
    }
}
