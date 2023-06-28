using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Ae.Nuntium.Destinations
{
    public sealed class RocketChatWebhookDestination : IExtractedPostDestination
    {
        public sealed class Configuration
        {
            public Uri WebhookAddress { get; set; }
            public int MaximumPostDelaySeconds { get; set; } = 20;
        }

        private readonly ILogger<RocketChatWebhookDestination> _logger;
        private readonly Configuration _configuration;

        public sealed class RocketChatPayload
        {
            public sealed class RocketChatAttachment
            {
                [JsonPropertyName("title")]
                public string Title { get; set; }
                [JsonPropertyName("title_link")]
                public string TitleLink { get; set; }
                [JsonPropertyName("text")]
                public string Text { get; set; }
                [JsonPropertyName("image_url")]
                public string ImageUrl { get; set; }
                [JsonPropertyName("color")]
                public string Color { get; set; }
            }

            [JsonPropertyName("text")]
            public string Text { get; set; }
            [JsonPropertyName("attachments")]
            public IList<RocketChatAttachment> Attachments { get; set; } = new List<RocketChatAttachment>();
        }

        public RocketChatWebhookDestination(ILogger<RocketChatWebhookDestination> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts)
        {
            using (var httpClient = new HttpClient())
            {
                foreach (var post in posts)
                {
                    var payload = new RocketChatPayload
                    {
                        Text = post.Author + ": " + post.Content,
                    };

                    foreach (var media in post.Media)
                    {
                        payload.Attachments.Add(new RocketChatPayload.RocketChatAttachment
                        {
                            ImageUrl = media.ToString(),
                            Title = post.Author,
                            TitleLink = media.ToString()
                        });
                    }

                    _logger.LogInformation("Posting {Link} to Rocket Chat", post.Permalink);
                    var response = await httpClient.PostAsJsonAsync(_configuration.WebhookAddress, payload);

                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}