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

            var seen = await ReadUris(cancellation);
            return posts.ExceptBy(seen, x => x.Permalink);
        }

        public async Task RemoveSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            var items = new List<Uri>();

            foreach (var permalink in await ReadUris(cancellation))
            {
                if (!posts.Select(x => x.Permalink).Contains(permalink))
                {
                    items.Add(permalink);
                }
            }

            await File.WriteAllLinesAsync(_configuration.File, items.Select(x => x.ToString()), cancellation);
        }

        private async Task<IEnumerable<Uri>> ReadUris(CancellationToken cancellation)
        {
            return (await File.ReadAllLinesAsync(_configuration.File, cancellation)).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new Uri(x));
        }
    }
}