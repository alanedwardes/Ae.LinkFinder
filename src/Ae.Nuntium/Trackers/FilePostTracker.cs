using Ae.Nuntium.Extractors;

namespace Ae.Nuntium.Trackers
{
    public sealed class FilePostTracker : IPostTracker
    {
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public string File { get; set; }
        }

        public FilePostTracker(Configuration configuration) => _configuration = configuration;

        public async Task SetSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken token)
        {
            await File.AppendAllLinesAsync(_configuration.File, posts.Select(x => x.Permalink.ToString()), token);
        }

        public async Task<IEnumerable<ExtractedPost>> GetUnseenPosts(IEnumerable<ExtractedPost> posts, CancellationToken token)
        {
            if (!File.Exists(_configuration.File))
            {
                return posts;
            }

            var seen = (await File.ReadAllLinesAsync(_configuration.File, token)).Select(x => new Uri(x));
            return posts.ExceptBy(seen, x => x.Permalink);
        }
    }
}