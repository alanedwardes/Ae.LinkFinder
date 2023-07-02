using Ae.Nuntium.Sources;

namespace Ae.Nuntium.Extractors
{
    /// <summary>
    /// Extracts posts from the given source.
    /// </summary>
    public interface IPostExtractor
    {
        /// <summary>
        /// Extract posts from the given source.
        /// </summary>
        /// <param name="sourceDocument">The source document to extract posts from.</param>
        /// <returns>A list of posts in chronological order (newest to oldest)</returns>
        Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument);
    }
}
