using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging.Abstractions;
using SmartReader;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class SmartReaderArticleEnricherTests
    {
        [Fact]
        public async Task TestEnrichExtractedPosts()
        {
            var article1Url = new Uri("https://example.com/blog/posts/vlc-remote-control-home-assistant/", UriKind.Absolute);
            var article2Url = new Uri("https://example.com/wiki/Jair_Bolsonaro", UriKind.Absolute);

            var factory = new MockHttpClientFactory(request =>
            {
                if (request.RequestUri == article1Url)
                {
                    return new HttpResponseMessage { Content = new StringContent(File.ReadAllText("Files/article1.html")) };
                }

                if (request.RequestUri == article2Url)
                {
                    return new HttpResponseMessage { Content = new StringContent(File.ReadAllText("Files/article2.html")) };
                }

                throw new InvalidOperationException();
            });

            Reader.SetBaseHttpClientHandler(factory.CreateHandler());

            var enricher = new SmartReaderArticleEnricher(NullLogger<SmartReaderArticleEnricher>.Instance, factory);

            var posts = new[] { new ExtractedPost(article1Url), new ExtractedPost(article2Url) };

            await enricher.EnrichExtractedPosts(posts, CancellationToken.None);

            posts.Compare("Files/articles1.json");
        }
    }
}
