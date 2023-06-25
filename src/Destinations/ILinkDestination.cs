using Ae.LinkFinder.Extractors;

namespace Ae.LinkFinder.Destinations
{
    public interface ILinkDestination
    {
        Task PostLinks(IEnumerable<ExtractedPost> posts);
    }
}