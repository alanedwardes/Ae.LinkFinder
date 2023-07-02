using Ae.Nuntium.Destinations;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public sealed class ContentFinder : IContentFinder
    {
        private readonly ILogger<ContentFinder> _logger;

        public ContentFinder(ILogger<ContentFinder> logger) => _logger = logger;

        public async Task FindContent(IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            _logger.LogInformation("Getting links with source {Source}", source);

            var content = await source.GetContent(cancellation);

            var posts = (await extractor.ExtractPosts(content)).Select((Post, Index) => (Post, Index)).ToList();
            if (posts.Count == 0)
            {
                _logger.LogWarning("Found no posts from source {Source}", source);
                return;
            }

            IList<Uri> unseenLinks = (await tracker.GetUnseenLinks(posts.Select(x => x.Post.Permalink), cancellation)).ToList();
            IList<ExtractedPost> unseenPosts = posts.Where(x => unseenLinks.Contains(x.Post.Permalink)).OrderByDescending(x => x.Index).Select(x => x.Post).ToList();
            if (unseenPosts.Count == 0)
            {
                _logger.LogInformation("All {Total} posts from {Source} were seen before", posts.Count, source);
                return;
            }

            _logger.LogInformation("Found {Unseen} unseen links of {Total} total from source {Source}", unseenLinks.Count, posts.Count, source);

            foreach (var destination in destinations)
            {
                await destination.ShareExtractedPosts(unseenPosts, cancellation);
            }

            await tracker.SetLinksSeen(unseenLinks, cancellation);
        }
    }
}