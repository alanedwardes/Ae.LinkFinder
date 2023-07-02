using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Enrichers
{
    /// <summary>
    /// Enriches a set of <see cref="ExtractedPost"/> with data not contained within the original source document.
    /// </summary>
    public interface IExtractedPostEnricher
    {
        /// <summary>
        /// Enrich each extracted post using data from other sources.
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task EnrichExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation);
    }
}
