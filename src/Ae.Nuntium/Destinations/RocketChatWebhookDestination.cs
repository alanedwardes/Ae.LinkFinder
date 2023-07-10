using Ae.Nuntium.Extractors;
using Ae.Nuntium.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
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
            [JsonPropertyName("alias")]
            public string? Alias { get; set; }
            [JsonPropertyName("avatar")]
            public Uri? Avatar { get; set; }
            [JsonPropertyName("text")]
            public string? Text { get; set; }
            [JsonPropertyName("attachments")]
            public IList<RocketChatAttachment>? Attachments { get; set; } = new List<RocketChatAttachment>();
        }

        public RocketChatWebhookDestination(ILogger<RocketChatWebhookDestination> logger, IHttpClientFactory httpClientFactory, Configuration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            foreach (var post in posts)
            {
                var parts = new[] { post.TextSummary, post.Permalink.ToString() }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                // https://docs.rocket.chat/use-rocket.chat/workspace-administration/integrations
                var payload = new RocketChatPayload
                {
                    Text = parts.Length == 0 ? null : string.Join("\n\n", parts),
                    Alias = post.Author
                };

                foreach (var media in post.Media)
                {
                    payload.Attachments?.Add(new RocketChatPayload.RocketChatAttachment
                    {
                        ImageUrl = media.ToString()
                    });
                }

                if (payload.Attachments?.Count == 0)
                {
                    payload.Attachments = null;
                }

                _logger.LogInformation("Posting {Link} to Rocket Chat", post.Permalink);
                using var response = await HttpClientExtensions.SendWrapped(httpClient.PostAsJsonAsync(_configuration.WebhookAddress, payload, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                }, cancellation));
            }
        }
    }
}