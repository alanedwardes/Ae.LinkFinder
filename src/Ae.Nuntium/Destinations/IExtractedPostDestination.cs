using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Destinations
{
    public interface IExtractedPostDestination
    {
        Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation);
    }
}