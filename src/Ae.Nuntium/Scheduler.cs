using Ae.Nuntium.Configuration;
using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Cronos;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public sealed class Scheduler : IScheduler
    {
        private readonly ILogger<Scheduler> _logger;
        private readonly IConfigurationDrivenServiceFactory _nuntiumServiceFactory;
        private readonly IContentFinder _nuntiumFinderRunner;

        public Scheduler(ILogger<Scheduler> logger, IConfigurationDrivenServiceFactory nuntiumServiceFactory, IContentFinder nuntiumFinderRunner)
        {
            _logger = logger;
            _nuntiumServiceFactory = nuntiumServiceFactory;
            _nuntiumFinderRunner = nuntiumFinderRunner;
        }

        public async Task Schedule(NuntiumConfiguration configuration, CancellationToken cancellation)
        {
            var tasks = new List<Task>();

            foreach (var finder in configuration.Finders.Where(x => !x.Skip))
            {
                var cron = CronExpression.Parse(finder.Cron);
                var source = _nuntiumServiceFactory.GetSource(finder.Source);
                var extractor = _nuntiumServiceFactory.GetExtractor(finder.Extractor);
                var tracker = _nuntiumServiceFactory.GetTracker(finder.Tracker);
                var enricher = finder.Enricher == null ? null : _nuntiumServiceFactory.GetEnricher(finder.Enricher);
                var destinations = finder.Destinations.Select(x => _nuntiumServiceFactory.GetDestination(x)).ToList();

                if (finder.Testing)
                {
                    _logger.LogInformation("Running {Source} in test mode", source);
                    await _nuntiumFinderRunner.FindContent(source, extractor, tracker, enricher, destinations, cancellation);
                }
                else
                {
                    tasks.Add(RunContinuously(cron, source, extractor, tracker, enricher, destinations, cancellation));
                }
            }

            _logger.LogInformation("Started {Tasks} tasks", tasks.Count);

            await Task.WhenAll(tasks);
        }

        public async Task RunContinuously(CronExpression cronExpression, IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IExtractedPostEnricher? enricher, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            do
            {
                DateTime nextUtc = cronExpression.GetNextOccurrence(DateTime.UtcNow) ?? throw new InvalidOperationException($"Unable to get next occurance of {cronExpression}");

                var delay = nextUtc - DateTime.UtcNow;

                _logger.LogInformation("Next occurance is {NextUtc}, waiting {Delay}", nextUtc, delay);

                await Task.Delay(delay, cancellation);

                try
                {
                    await _nuntiumFinderRunner.FindContent(source, extractor, tracker, enricher, destinations, cancellation);
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