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

        public Task ShareExtractedPosts(IEnumerable<ExtractedPost> posts)
        {
            foreach (var post in posts)
            {
                _logger.LogInformation("{Author}: {Content} {Permalink}", post.Author, post.Content, post.Permalink);
            }

            return Task.CompletedTask;
        }
    }
}