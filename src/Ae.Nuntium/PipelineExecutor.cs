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

        public async Task RunPipeline(IList<IContentSource> sources, IList<IPostExtractor> extractors, IPostTracker tracker, IList<IExtractedPostEnricher> enrichers, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            _logger.LogInformation("Getting links with sources {Sources}", string.Join(", ", sources.Select(x => x.ToString())));

            var documents = await Task.WhenAll(sources.Select(x => x.GetContent(cancellation)));

            var extractionTasks = documents.Select(document => extractors.Select(extractor => extractor.ExtractPosts(document)));

            var allExtractedPosts = (await Task.WhenAll(extractionTasks.SelectMany(x => x))).SelectMany(x => x);

            var posts = allExtractedPosts.Select((Post, Index) => (Post, Index)).ToList();
            if (posts.Count == 0)
            {
                _logger.LogWarning("Found no posts from source {Sources}", string.Join(", ", sources.Select(x => x.ToString())));
                return;
            }

            var duplicatedPosts = posts.GroupBy(x => x.Post.Permalink).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicatedPosts.Count > 0)
            {
                _logger.LogWarning("Source {Sources} found duplicated posts with the same set of URIs: {DuplicatedPermalinks} (all will be posted)", string.Join(", ", sources.Select(x => x.ToString())), string.Join(", ", duplicatedPosts));
            }

            IList<ExtractedPost> unseenLinks = (await tracker.GetUnseenPosts(posts.Select(x => x.Post), cancellation)).ToList();
            IList<ExtractedPost> unseenPosts = posts.Where(x => unseenLinks.Contains(x.Post)).OrderByDescending(x => x.Index).Select(x => x.Post).ToList();
            if (unseenPosts.Count == 0)
            {
                _logger.LogInformation("All {Total} posts from {Sources} were seen before", posts.Count, string.Join(", ", sources.Select(x => x.ToString())));
                return;
            }

            // Enrich the posts, if an enricher was supplied
            foreach (var enricher in enrichers)
            {
                await enricher.EnrichExtractedPosts(unseenPosts, cancellation);
            }

            _logger.LogInformation("Found {Unseen} unseen posts of {Total} total from source {Sources}", unseenLinks.Count, posts.Count, string.Join(", ", sources.Select(x => x.ToString())));

            foreach (var destination in destinations)
            {
                await destination.ShareExtractedPosts(unseenPosts, cancellation);
            }

            await tracker.SetSeenPosts(unseenLinks, cancellation);
        }
    }
}