using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Enrichers
{
    public sealed class WhitespaceRemovalEnricher : IExtractedPostEnricher
    {
        public Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            foreach (var post in posts)
            {
                post.Title = string.IsNullOrWhiteSpace(post.Title) ? null : post.Title.Trim();
                post.Author = string.IsNullOrWhiteSpace(post.Author) ? null : post.Author.Trim();
                post.TextSummary = string.IsNullOrWhiteSpace(post.TextSummary) ? null : post.TextSummary.Trim();
                post.RawContent = string.IsNullOrWhiteSpace(post.RawContent) ? null : post.RawContent.Trim();
            }

            return Task.CompletedTask;
        }
    }
}
