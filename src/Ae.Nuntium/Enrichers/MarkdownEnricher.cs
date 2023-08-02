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
                if (post.TextSummary != null)
                {
                    post.TextSummary = converter.Convert(post.TextSummary);
                }

                if (post.RawContent != null)
                {
                    post.RawContent = converter.Convert(post.RawContent);
                }
            }

            return Task.CompletedTask;
        }
    }
}
