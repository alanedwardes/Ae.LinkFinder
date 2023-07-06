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

        public async Task SetSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            await File.AppendAllLinesAsync(_configuration.File, posts.Select(x => x.Permalink.ToString()), cancellation);
        }

        public async Task<IEnumerable<ExtractedPost>> GetUnseenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            if (!File.Exists(_configuration.File))
            {
                return posts;
            }

            var seen = (await File.ReadAllLinesAsync(_configuration.File, cancellation)).Select(x => new Uri(x));
            return posts.ExceptBy(seen, x => x.Permalink);
        }

        public Task RemoveSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}