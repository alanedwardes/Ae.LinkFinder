using Ae.Nuntium.Extractors;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SmartReader;

namespace Ae.Nuntium.Enrichers
{
    public sealed class SmartReaderArticleEnricher : IExtractedPostEnricher
    {
        private readonly ILogger<SmartReaderArticleEnricher> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SmartReaderArticleEnricher(ILogger<SmartReaderArticleEnricher> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            using var httpClient = _httpClientFactory.CreateClient("GZIP_CLIENT");

            foreach (var post in posts)
            {
                try
                {
                    await EnrichExtractedPost(httpClient, post, cancellation);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to enrich post {Post}", post);
                }
            }
        }

        private static async Task EnrichExtractedPost(HttpClient httpClient, ExtractedPost post, CancellationToken cancellation)
        {
            var article = Reader.ParseArticle(post.Permalink.ToString(), await httpClient.GetStreamAsync(post.Permalink, cancellation));

            post.Title ??= article.Title;
            post.Author ??= article.Author;
            post.Summary ??= article.Excerpt;
            post.Body ??= article.Content;

            if (post.Body != null)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(post.Body);
                htmlDoc.DocumentNode.GetLinksAndMedia(post.Permalink, foundLink => post.Links.Add(foundLink), foundMedia => post.Media.Add(foundMedia));
            }
        }
    }
}
