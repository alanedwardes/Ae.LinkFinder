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
        }

        private readonly ILogger<RocketChatWebhookDestination> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Configuration _configuration;

        public sealed class RocketChatPayload
        {
            public sealed class RocketChatAttachment
            {
                [JsonPropertyName("title")]
                public string? Title { get; set; }
                [JsonPropertyName("title_link")]
                public string? TitleLink { get; set; }
                [JsonPropertyName("text")]
                public string? Text { get; set; }
                [JsonPropertyName("image_url")]
                public string? ImageUrl { get; set; }
                [JsonPropertyName("color")]
                public string? Color { get; set; }
            }

            [JsonPropertyName("text")]
            public string? Text { get; set; }
            [JsonPropertyName("attachments")]
            public IList<RocketChatAttachment> Attachments { get; set; } = new List<RocketChatAttachment>();
        }

        public RocketChatWebhookDestination(ILogger<RocketChatWebhookDestination> logger, IHttpClientFactory httpClientFactory, Configuration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            foreach (var post in posts)
            {
                var text = string.Join(": ", post.Author, post.TextSummary ?? post.Permalink?.ToString());

                // https://docs.rocket.chat/use-rocket.chat/workspace-administration/integrations
                var payload = new RocketChatPayload
                {
                    Text = post.Permalink == null ? text : text + "\n\n" + post.Permalink.ToString()
                };

                foreach (var media in post.Media)
                {
                    payload.Attachments.Add(new RocketChatPayload.RocketChatAttachment
                    {
                        ImageUrl = media.ToString()
                    });
                }

                _logger.LogInformation("Posting {Link} to Rocket Chat", post.Permalink);
                using var response = await httpClient.PostAsJsonAsync(_configuration.WebhookAddress, payload);

                response.EnsureSuccessStatusCode();
            }
        }
    }
}