using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium.Destinations
{
    public sealed class ConsoleDestination : IExtractedPostDestination
    {
        private readonly ILogger<ConsoleDestination> _logger;

        public ConsoleDestination(ILogger<ConsoleDestination> logger)
        {
            _logger = logger;
        }

        public Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            foreach (var post in posts)
            {
                _logger.LogInformation(post.ToString());
            }

            return Task.CompletedTask;
        }
    }
}