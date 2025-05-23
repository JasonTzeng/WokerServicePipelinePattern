using WorkerServicePipeline.Models;

namespace WorkerServicePipeline.Apis.Interfaces
{
    public interface IFakeApiClient
    {
        Task<IEnumerable<Post>> GetPostsAsync();
        Task<Post?> AddPostsAsync(Post post);
    }
}
