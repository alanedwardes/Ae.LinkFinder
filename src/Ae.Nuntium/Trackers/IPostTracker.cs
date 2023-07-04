using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Trackers
{
    public interface IPostTracker
    {
        Task<IEnumerable<ExtractedPost>> GetUnseenPosts(IEnumerable<ExtractedPost> posts, CancellationToken token);
        Task SetSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken token);
    }
}