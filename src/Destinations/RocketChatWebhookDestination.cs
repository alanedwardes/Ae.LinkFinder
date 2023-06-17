using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Ae.LinkFinder.Destinations
{
    public sealed class RocketChatWebhookDestination : ILinkDestination
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
            public string Text { get; set; }
        }

        public RocketChatWebhookDestination(ILogger<RocketChatWebhookDestination> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task PostLinks(ISet<Uri> links)
        {
            var random = new Random();

            using (var httpClient = new HttpClient())
            {
                foreach (var link in links)
                {
                    var payload = new RocketChatPayload
                    {
                        Text = link.ToString()
                    };

                    _logger.LogInformation("Posting {Link} to Rocket Chat", link);
                    var response = await httpClient.PostAsJsonAsync(_configuration.WebhookAddress, payload);

                    response.EnsureSuccessStatusCode();

                    if (links.Count > 0)
                    {
                        // To prevent spamming links (and causing a lot of requests to get previews etc)
                        await Task.Delay(TimeSpan.FromSeconds(random.NextDouble() * _configuration.MaximumPostDelaySeconds));
                    }
                }
            }
        }
    }
}