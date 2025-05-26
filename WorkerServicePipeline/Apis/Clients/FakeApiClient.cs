using WorkerServicePipeline.Apis.Interfaces;
using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Apis.Clients
{
    public class FakeApiClient : IFakeApiClient
    {
        private readonly ILogger<FakeApiClient> _logger;
        private readonly HttpClient _httpClient;

        public FakeApiClient(ILogger<FakeApiClient> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        public async Task<Post?> AddPostsAsync(Post post)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/posts", post);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Post>();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post to FakeApi");
                throw;
            }
        }
        public async Task<IEnumerable<Post>> GetPostsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/posts");
                response.EnsureSuccessStatusCode();
                var posts = await response.Content.ReadFromJsonAsync<IEnumerable<Post>>();
                return posts ?? Enumerable.Empty<Post>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching posts from FakeApi");
                throw;
            }
        }
    }
}
