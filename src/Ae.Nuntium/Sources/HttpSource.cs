using Microsoft.Extensions.Logging;

namespace Ae.Nuntium.Sources
{
    public sealed class HttpSource : IContentSource
    {
        private readonly ILogger<HttpSource> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public Uri FeedAddress { get; set; }
        }

        public HttpSource(ILogger<HttpSource> logger, IHttpClientFactory httpClientFactory, Configuration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<SourceDocument> GetContent(CancellationToken token)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(_configuration.FeedAddress, token);
            response.EnsureSuccessStatusCode();

            return new SourceDocument
            {
                Body = await response.Content.ReadAsStringAsync(token),
                Source = response.RequestMessage.RequestUri
            };
        }
    }
}