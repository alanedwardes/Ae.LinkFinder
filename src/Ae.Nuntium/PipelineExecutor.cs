using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public sealed class PipelineExecutor : IPipelineExecutor
    {
        private readonly ILogger<PipelineExecutor> _logger;

        public PipelineExecutor(ILogger<PipelineExecutor> logger) => _logger = logger;

        public async Task RunPipeline(IContentSource source, IPostExtractor extractor, IPostTracker tracker, IList<IExtractedPostEnricher> enrichers, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            _logger.LogInformation("Getting links with source {Source}", source);

            var content = await source.GetContent(cancellation);

            var posts = (await extractor.ExtractPosts(content)).Select((Post, Index) => (Post, Index)).ToList();
            if (posts.Count == 0)
            {
                _logger.LogWarning("Found no posts from source {Source}", source);
                return;
            }

            var duplicatedPosts = posts.GroupBy(x => x.Post.Permalink).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicatedPosts.Count > 0)
            {
                _logger.LogWarning("Source {Source} found duplicated posts with the same set of URIs: {DuplicatedPermalinks} (all will be posted)", source, string.Join(", ", duplicatedPosts));
            }

            IList<ExtractedPost> unseenLinks = (await tracker.GetUnseenPosts(posts.Select(x => x.Post), cancellation)).ToList();
            IList<ExtractedPost> unseenPosts = posts.Where(x => unseenLinks.Contains(x.Post)).OrderByDescending(x => x.Index).Select(x => x.Post).ToList();
            if (unseenPosts.Count == 0)
            {
                _logger.LogInformation("All {Total} posts from {Source} were seen before", posts.Count, source);
                return;
            }

            // Enrich the posts, if an enricher was supplied
            foreach (var enricher in enrichers)
            {
                await enricher.EnrichExtractedPosts(unseenPosts, cancellation);
            }

            _logger.LogInformation("Found {Unseen} unseen links of {Total} total from source {Source}", unseenLinks.Count, posts.Count, source);

            foreach (var destination in destinations)
            {
                await destination.ShareExtractedPosts(unseenPosts, cancellation);
            }

            await tracker.SetSeenPosts(unseenLinks, cancellation);
        }
    }
}