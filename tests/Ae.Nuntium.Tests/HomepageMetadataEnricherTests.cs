using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class HomepageMetadataEnricherTests
    {
        [Theory]
        [InlineData("Files/article1.html", "Casting Video to VLC via Home Assistant - Alan Edwardes", "https://alan.gdn/0c13dab9-20eb-4787-bdd3-a9ec9755d422.png")]
        [InlineData("Files/article2.html", "Jair Bolsonaro - Wikipedia", "https://www.example.com/static/apple-touch/wikipedia.png")]
        [InlineData("Files/article3.html", "ESA/Webb Space Telescope", "https://cdn.esawebb.org/archives/images/screen/ann2203a.jpg")]
        [InlineData("Files/article4.html", "NASA", "https://www.example.com/sites/all/themes/custom/nasatwo/images/apple-touch-icon.png")]
        [InlineData("Files/article5.html", "Wikipedia, the free encyclopedia", "https://www.example.com/static/apple-touch/wikipedia.png")]
        [InlineData("Files/article6.html", "GitHub", "https://github.githubassets.com/images/modules/open_graph/github-logo.png")]
        [InlineData("Files/article7.html", "Microsoft – Cloud, Computers, Apps & Gaming", "https://s2.googleusercontent.com/s2/favicons?domain=www.example.com&sz=32")]
        public async Task TestHomepageMetadataEnricher(string file, string title, string avatar)
        {
            var factory = new MockHttpClientFactory(request =>
            {
                if (request.RequestUri == new Uri("https://www.example.com/"))
                {
                    return new HttpResponseMessage { Content = new StringContent(File.ReadAllText(file)) };
                }

                throw new InvalidOperationException();
            });

            var enricher = new HomepageMetadataEnricher(factory);

            var post = new ExtractedPost(new Uri("https://www.example.com/test"));

            await enricher.EnrichExtractedPosts(new[] { post }, CancellationToken.None);

            Assert.Equal(title, post.Author);
            Assert.Equal(avatar, post.Avatar.AbsoluteUri);
        }
    }
}
