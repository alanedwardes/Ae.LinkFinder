using Ae.Nuntium.Destinations;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Cronos;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public sealed class NuntiumFinderRunner : INuntiumFinderRunner
    {
        private readonly ILogger<NuntiumFinderRunner> _logger;

        public NuntiumFinderRunner(ILogger<NuntiumFinderRunner> logger)
        {
            _logger = logger;
        }

        public async Task RunOnce(IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            _logger.LogInformation("Getting links with source {Source}", source);

            var content = await source.GetContent(cancellation);

            var posts = (await extractor.ExtractPosts(content)).Where(x => x.HasContent()).ToList();

            if (posts.Count == 0)
            {
                _logger.LogWarning("Found no posts from source: {Source}", source);
                return;
            }

            var unseen = await tracker.GetUnseenLinks(posts.Select(x => x.Permalink), cancellation);

            _logger.LogInformation("Found {Unseen} unseen links of {Total} total", unseen.Count(), posts.Count);

            foreach (var destination in destinations)
            {
                await destination.ShareExtractedPosts(posts.Where(x => unseen.Contains(x.Permalink)), cancellation);
            }

            await tracker.SetLinksSeen(unseen, cancellation);
        }

        public async Task RunContinuously(CronExpression cronExpression, IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            do
            {
                DateTime nextUtc = cronExpression.GetNextOccurrence(DateTime.UtcNow) ?? throw new InvalidOperationException($"Unable to get next occurance of {cronExpression}");

                var delay = nextUtc - DateTime.UtcNow;

                _logger.LogInformation("Next occurance is {NextUtc}, waiting {Delay}", nextUtc, delay);

                await Task.Delay(delay, cancellation);

                try
                {
                    await RunOnce(source, extractor, tracker, destinations, cancellation);
                }
                catch (Exception ex) when (!cancellation.IsCancellationRequested)
                {
                    _logger.LogCritical(ex, "Exception from finder");
                }
            }
            while (!cancellation.IsCancellationRequested);
        }
    }
}