﻿using Ae.Nuntium.Destinations;
using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class RocketChatWebhookDestinationTests
    {
        private readonly RocketChatWebhookDestination _destination;
        private HttpRequestMessage? _requestMessage = null;

        public RocketChatWebhookDestinationTests()
        {
            _destination = new RocketChatWebhookDestination(NullLogger<RocketChatWebhookDestination>.Instance, new MockHttpClientFactory(request =>
            {
                _requestMessage = request;
                return new HttpResponseMessage();
            }), new RocketChatWebhookDestination.Configuration
            {
                WebhookAddress = new Uri("https://www.example.com/", UriKind.Absolute)
            });
        }

        [Fact]
        public async Task TestPostLink()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost
                {
                    Permalink = new Uri("https://www.example.com/", UriKind.Absolute)
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"text\":\"https://www.example.com/\"}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestFullPost()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost
                {
                    Permalink = new Uri("https://www.example.com/", UriKind.Absolute),
                    RawContent = "<html>",
                    TextSummary = "hello this is a summary",
                    Author = "wibble",
                    Title = "Title",
                    Media = new HashSet<Uri>
                    {
                        new Uri("https://www.example.com/test.jpg")
                    }
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"text\":\"wibble: hello this is a summary\\n\\nhttps://www.example.com/\",\"attachments\":[{\"image_url\":\"https://www.example.com/test.jpg\"}]}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestPartialPost()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost
                {
                    Permalink = new Uri("https://www.example.com/", UriKind.Absolute),
                    TextSummary = "hello this is a summary"
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"text\":\"hello this is a summary\\n\\nhttps://www.example.com/\"}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestPartialPostNoPermalink()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost
                {
                    TextSummary = "hello this is a summary"
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"text\":\"hello this is a summary\"}", await _requestMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestOnlyMedia()
        {
            await _destination.ShareExtractedPosts(new List<ExtractedPost>
            {
                new ExtractedPost
                {
                    Media = new HashSet<Uri>
                    {
                        new Uri("https://www.example.com/test.jpg")
                    }
                }
            }, CancellationToken.None);

            Assert.Equal("https://www.example.com/", _requestMessage.RequestUri.ToString());
            Assert.Equal("{\"attachments\":[{\"image_url\":\"https://www.example.com/test.jpg\"}]}", await _requestMessage.Content.ReadAsStringAsync());
        }
    }
}
