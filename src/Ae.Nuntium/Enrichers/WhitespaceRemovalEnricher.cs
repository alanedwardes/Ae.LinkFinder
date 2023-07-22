using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Enrichers
{
    public sealed class WhitespaceRemovalEnricher : IExtractedPostEnricher
    {
        public Task EnrichExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            foreach (var post in posts)
            {
                post.Title = string.IsNullOrWhiteSpace(post.Title) ? null : post.Title.Trim();
                post.Author = string.IsNullOrWhiteSpace(post.Author) ? null : post.Author.Trim();
                post.SummaryContent = string.IsNullOrWhiteSpace(post.SummaryContent) ? null : post.SummaryContent.Trim();
                post.FullContent = string.IsNullOrWhiteSpace(post.FullContent) ? null : post.FullContent.Trim();
            }

            return Task.CompletedTask;
        }
    }
}
