using Ae.Nuntium.Extractors;
using ReverseMarkdown;

namespace Ae.Nuntium.Enrichers
{
    public sealed class SummaryMarkdownEnricher : IExtractedPostEnricher
    {
        public Task EnrichExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            var converter = new Converter();

            foreach (var post in posts)
            {
                if (string.IsNullOrWhiteSpace(post.SummaryContent))
                {
                    continue;
                }

                var converted = converter.Convert(post.SummaryContent);
                if (string.IsNullOrWhiteSpace(converted))
                {
                    continue;
                }

                post.SummaryContent = converted;
            }

            return Task.CompletedTask;
        }
    }
}
