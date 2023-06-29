using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
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
            [JsonPropertyName("username")]
            public string Username { get; set; }
            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        public DiscordWebhookDestination(ILogger<DiscordWebhookDestination> logger, IHttpClientFactory httpClientFactory, Configuration configuration)
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
                // https://discord.com/developers/docs/resources/webhook#execute-webhook
                var payload = new DiscordPayload
                {
                    Username = post.Author,
                    Content = post.Permalink.ToString()
                };

                _logger.LogInformation("Posting {Link} to Discord", post.Permalink);
                using var response = await httpClient.PostAsJsonAsync(_configuration.WebhookAddress, payload);

                response.EnsureSuccessStatusCode();
            }
        }
    }
}