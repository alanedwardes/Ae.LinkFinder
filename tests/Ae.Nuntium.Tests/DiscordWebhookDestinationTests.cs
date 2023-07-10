using Ae.Nuntium.Destinations;
using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class DiscordWebhookDestinationTests
    {
        private readonly DiscordWebhookDestination _destination;
        private HttpRequestMessage? _requestMessage = null;

        public DiscordWebhookDestinationTests()
        {
            _destination = new DiscordWebhookDestination(NullLogger<DiscordWebhookDestination>.Instance, new MockHttpClientFactory(request =>
            {
                _requestMessage = request;
                return new HttpResponseMessage();
            }), new DiscordWebhookDestination.Configuration
            {
                WebhookAddress = new Uri("https://www.example.com/", UriKind.Absolute)
            });
        }

        [Fact]
        public async Task TestPostLink()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost(new Uri("https://www.example.com/", UriKind.Absolute))
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"embeds\":[{\"url\":\"https://www.example.com/\"}]}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestFullPost()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost(new Uri("https://www.example.com/", UriKind.Absolute))
                {
                    RawContent = "<html>",
                    TextSummary = "hello this is a summary",
                    Author = "wibble",
                    Avatar = new Uri("https://www.example.com/avatar.png"),
                    Title = "Title",
                    Media = new HashSet<Uri>
                    {
                        new Uri("https://www.example.com/test.jpg")
                    }
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"username\":\"wibble\",\"avatar_url\":\"https://www.example.com/avatar.png\",\"embeds\":[{\"title\":\"Title\",\"description\":\"hello this is a summary\",\"url\":\"https://www.example.com/\"},{\"image\":{\"url\":\"https://www.example.com/test.jpg\"}}]}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestPartialPost()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost(new Uri("https://www.example.com/", UriKind.Absolute))
                {
                    TextSummary = "hello this is a summary"
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"embeds\":[{\"description\":\"hello this is a summary\",\"url\":\"https://www.example.com/\"}]}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestOnlyMedia()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost(new Uri("https://www.example.com/", UriKind.Absolute))
                {
                    Media = new HashSet<Uri>
                    {
                        new Uri("https://www.example.com/test.jpg")
                    }
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"embeds\":[{\"url\":\"https://www.example.com/\"},{\"image\":{\"url\":\"https://www.example.com/test.jpg\"}}]}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestLimits()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost(new Uri("https://www.example.com/", UriKind.Absolute))
                {
                    Title = new string('*', 10000),
                    TextSummary = new string('*', 10000),
                    Media = Enumerable.Range(1, 20).Select(x => new Uri(x.ToString(), UriKind.Relative)).ToHashSet()
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"embeds\":[{\"title\":\"***************************************************************************************************************************************************************************************************************************************************************\\u2026\",\"description\":\"***************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************\\u2026\",\"url\":\"https://www.example.com/\"},{\"image\":{\"url\":\"1\"}},{\"image\":{\"url\":\"2\"}},{\"image\":{\"url\":\"3\"}},{\"image\":{\"url\":\"4\"}},{\"image\":{\"url\":\"5\"}},{\"image\":{\"url\":\"6\"}},{\"image\":{\"url\":\"7\"}},{\"image\":{\"url\":\"8\"}},{\"image\":{\"url\":\"9\"}},{\"image\":{\"url\":\"10\"}}]}", await _requestMessage.Content.ReadAsStringAsync());
        }
    }
}
