namespace Ae.LinkFinder.Trackers
{
    public sealed class FileLinkTracker : ILinkTracker
    {
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public string File { get; set; }
        }

        public FileLinkTracker(Configuration configuration) => _configuration = configuration;

        public async Task SetLinksSeen(IEnumerable<Uri> links, CancellationToken token)
        {
            await File.AppendAllLinesAsync(_configuration.File, links.Select(x => x.ToString()), token);
        }

        public async Task<IEnumerable<Uri>> GetUnseenLinks(IEnumerable<Uri> links, CancellationToken token)
        {
            if (!File.Exists(_configuration.File))
            {
                return links;
            }

            var seen = (await File.ReadAllLinesAsync(_configuration.File)).Select(x => new Uri(x));
            return links.Except(seen).ToHashSet();
        }
    }
}