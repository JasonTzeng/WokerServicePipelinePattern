using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Apis.Interfaces;
using WorkerServicePipeline.Instrumentation;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Pipelines.Steps
{
    public class Enrich2 : IStep
    {
        private readonly ILogger<Enrich2> _logger;
        private readonly IFakeApiClient _fakeApiClient;
        private readonly CaseContext _context;

        public Enrich2(ILogger<Enrich2> logger,
            IFakeApiClient fakeApiClient,
            CaseContext context)
        {
            _logger = logger;
            _fakeApiClient = fakeApiClient;
            _context = context;
        }
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("Enrich2Execution");
            activity?.SetTag("step", "Enrich2");

            _logger.LogInformation("Enrich2 step started.");

            try
            {
                Post? post = await _fakeApiClient.AddPostsAsync(new Post { UserId = 5, Body = "test_body", Title = "test_title" });
                if (post != null)
                {
                    _context.EnrichedData["Enrich2_Post"] = post;
                    _logger.LogInformation("Enrich2 step completed successfully. Stored PostId: {PostId}, Title: {Title}", post.Id, post.Title);
                }
                else
                {
                    _logger.LogWarning("Enrich2 step: No post returned from API.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enrich2 step encountered an error.");
            }
        }
    }
}
