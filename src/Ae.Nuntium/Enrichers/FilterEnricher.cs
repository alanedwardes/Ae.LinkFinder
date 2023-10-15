using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium.Enrichers
{
    public sealed class FilterEnricher : IExtractedPostEnricher
    {
        private readonly ILogger<FilterEnricher> _logger;
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public int? MaxAgeDays { get; set; }
        }

        public FilterEnricher(ILogger<FilterEnricher> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            var toRemove = new List<ExtractedPost>();

            if (_configuration.MaxAgeDays.HasValue)
            {
                foreach (var post in posts)
                {
                    if (post.Published.HasValue)
                    {
                        if (DateTime.UtcNow - post.Published.Value > TimeSpan.FromDays(_configuration.MaxAgeDays.Value))
                        {
                            toRemove.Add(post);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Post {Post} has no published date, so MaxAgeDays={MaxAgeDays} filter will have no effect", post.Permalink, _configuration.MaxAgeDays);
                    }
                }
            }

            foreach (var remove in toRemove)
            {
                _logger.LogInformation("Post {Post} is older than MaxAgeDays={MaxAgeDays}, skipping", remove.Permalink, _configuration.MaxAgeDays);
                posts.Remove(remove);
            }

            return Task.CompletedTask;
        }
    }
}
