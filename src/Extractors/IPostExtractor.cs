using Ae.LinkFinder.Sources;

namespace Ae.LinkFinder.Extractors
{
    public interface IPostExtractor
    {
        Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument);
    }
}
