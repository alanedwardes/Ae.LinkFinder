using Ae.LinkFinder.Extractors;

namespace Ae.LinkFinder.Destinations
{
    public interface IExtractedPostDestination
    {
        Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts);
    }
}