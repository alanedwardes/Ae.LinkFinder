using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging;
using ReverseMarkdown;

namespace Ae.Nuntium.Enrichers
{
    public sealed class MarkdownEnricher : IExtractedPostEnricher
    {
        private readonly ILogger<MarkdownEnricher> _logger;

        public MarkdownEnricher(ILogger<MarkdownEnricher> logger) => _logger = logger;

        public Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            var converter = new Converter();

            foreach (var post in posts)
            {
                try
                {
                    ProcessPost(converter, post);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to enrich {Post}", post);
                }
            }

            return Task.CompletedTask;
        }

        private static void ProcessPost(Converter converter, ExtractedPost post)
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
    }
}
