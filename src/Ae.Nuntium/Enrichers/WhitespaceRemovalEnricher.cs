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
                post.Summary = string.IsNullOrWhiteSpace(post.Summary) ? null : post.Summary.Trim();
                post.Body = string.IsNullOrWhiteSpace(post.Body) ? null : post.Body.Trim();
            }

            return Task.CompletedTask;
        }
    }
}
