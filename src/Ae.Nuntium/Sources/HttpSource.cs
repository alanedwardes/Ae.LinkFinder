using Ae.Nuntium.Services;
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
            public Uri Address { get; set; }
        }

        public HttpSource(ILogger<HttpSource> logger, IHttpClientFactory httpClientFactory, Configuration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<SourceDocument> GetContent(CancellationToken cancellation)
        {
            using var httpClient = _httpClientFactory.CreateClient("GZIP_CLIENT");
            using var response = await httpClient.GetAsync(_configuration.Address, cancellation);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response status code for GET {_configuration.Address} does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
            }

            return new SourceDocument
            {
                Body = await response.Content.ReadAsStringAsync(cancellation),
                Address = response.RequestMessage.RequestUri
            };
        }

        public override string ToString() => _configuration.Address.ToString();
    }
}