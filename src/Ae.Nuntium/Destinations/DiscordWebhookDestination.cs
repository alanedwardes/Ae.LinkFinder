using Ae.Nuntium.Extractors;
using Ae.Nuntium.Services;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ae.Nuntium.Destinations
{
    public sealed class DiscordWebhookDestination : IExtractedPostDestination
    {
        public sealed class Configuration
        {
            public Uri WebhookAddress { get; set; }
        }

        private readonly ILogger<DiscordWebhookDestination> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Configuration _configuration;

        public sealed class DiscordPayload
        {
            public sealed class DiscordMedia
            {
                [JsonPropertyName("url")]
                public string? Url { get; set; }
            }

            public sealed class DiscordEmbed
            {
                [JsonPropertyName("title")]
                public string? Title { get; set; }
                [JsonPropertyName("description")]
                public string? Description { get; set; }
                [JsonPropertyName("url")]
                public string? Url { get; set; }
                [JsonPropertyName("image")]
                public DiscordMedia? Image { get; set; }
            }

            [JsonPropertyName("username")]
            public string? Username { get; set; }
            [JsonPropertyName("content")]
            public string? Content { get; set; }
            [JsonPropertyName("avatar_url")]
            public Uri? AvatarUrl { get; set; }
            [JsonPropertyName("embeds")]
            public IList<DiscordEmbed> Embeds { get; set; } = new List<DiscordEmbed>();
        }

        public DiscordWebhookDestination(ILogger<DiscordWebhookDestination> logger, IHttpClientFactory httpClientFactory, Configuration configuration)
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
                await SharePost(httpClient, post, cancellation);
            }
        }

        private async Task SharePost(HttpClient httpClient, ExtractedPost post, CancellationToken cancellation)
        {
            // https://discord.com/developers/docs/resources/webhook#execute-webhook
            var payload = new DiscordPayload
            {
                Username = post.Author,
                AvatarUrl = post.Avatar
            };

            // An embed must have either a "title" or a "description"
            // If one is not present, we can't use embeds
            if (post.Title == null && post.SummaryContent == null)
            {
                payload.Content = post.Permalink.ToString();
            }
            else
            {
                // Limits: https://discord.com/developers/docs/resources/channel#embed-object-embed-limits
                var embed = new DiscordPayload.DiscordEmbed
                {
                    Title = post.Title.Truncate(256),
                    Description = post.SummaryContent.Truncate(4096),
                    Url = post.Permalink.ToString()
                };

                if (post.Thumbnail != null)
                {
                    embed.Image = new DiscordPayload.DiscordMedia
                    {
                        Url = post.Thumbnail.ToString(),
                    };
                }

                payload.Embeds.Add(embed);
            }

            if (post.Thumbnail == null)
            {
                // Limits: https://discord.com/developers/docs/resources/webhook#execute-webhook-jsonform-params
                foreach (var media in post.Media.Take(10))
                {
                    payload.Embeds.Add(new DiscordPayload.DiscordEmbed
                    {
                        Image = new DiscordPayload.DiscordMedia
                        {
                            Url = media.ToString()
                        }
                    });
                }
            }

            _logger.LogInformation("Posting {Link} to Discord", post.Permalink);
            using var response = await HttpClientExtensions.SendWrapped(httpClient.PostAsJsonAsync(_configuration.WebhookAddress, payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            }, cancellation));
        }
    }
}