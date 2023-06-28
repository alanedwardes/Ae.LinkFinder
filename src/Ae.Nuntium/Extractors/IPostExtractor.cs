using Ae.Nuntium.Sources;

namespace Ae.Nuntium.Extractors
{
    public interface IPostExtractor
    {
        Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument);
    }
}
