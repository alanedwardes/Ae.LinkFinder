using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Enrichers
{
    public sealed class MarkdownEnricher : IExtractedPostEnricher
    {
        public Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            var converter = new ReverseMarkdown.Converter();

            foreach (var post in posts)
            {
                if (post.Summary != null)
                {
                    post.Summary = converter.Convert(post.Summary);
                }

                if (post.Body != null)
                {
                    post.Body = converter.Convert(post.Body);
                }
            }

            return Task.CompletedTask;
        }
    }
}
