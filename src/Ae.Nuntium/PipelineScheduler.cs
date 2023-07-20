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
    public sealed class PipelineScheduler : IPipelineScheduler
    {
        private readonly ILogger<PipelineScheduler> _logger;
        private readonly IPipelineServiceFactory _serviceFactory;
        private readonly IPipelineExecutor _pipelineExecutor;

        public PipelineScheduler(ILogger<PipelineScheduler> logger, IPipelineServiceFactory serviceFactory, IPipelineExecutor pipelineExecutor)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _pipelineExecutor = pipelineExecutor;
        }

        public async Task Schedule(MainConfiguration configuration, CancellationToken cancellation)
        {
            var tasks = new List<Task>();

            foreach (var pipeline in configuration.Pipelines.Where(x => !x.Skip))
            {
                var sources = pipeline.SourcesMarshaled.Select(x => _serviceFactory.GetSource(x)).ToList();
                var extractors = pipeline.ExtractorsMarshaled.Select(x => _serviceFactory.GetExtractor(x)).ToList();
                var tracker = _serviceFactory.GetTracker(pipeline.Tracker ?? new());
                var enrichers = pipeline.EnrichersMarshaled.Select(x => _serviceFactory.GetEnricher(x)).ToList();
                var destinations = pipeline.DestinationsMarshaled.Select(x => _serviceFactory.GetDestination(x)).ToList();

                if (pipeline.Testing)
                {
                    _logger.LogInformation("Running {Sources} in test mode", string.Join(", ", sources.Select(x => x.ToString())));
                    await _pipelineExecutor.RunPipeline(sources, extractors, tracker, enrichers, destinations, cancellation);
                }
                else
                {
                    tasks.Add(RunContinuously(pipeline, sources, extractors, tracker, enrichers, destinations, cancellation));
                }
            }

            _logger.LogInformation("Started {Tasks} tasks", tasks.Count);

            await Task.WhenAll(tasks);
        }

        public async Task RunContinuously(PipelineConfiguration pipelineConfiguration, IList<IContentSource> sources, IList<IPostExtractor> extractor, IPostTracker tracker, IList<IExtractedPostEnricher> enrichers, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            var cron = CronExpression.Parse(pipelineConfiguration.Cron);
            var random = new Random();

            do
            {
                DateTime nextUtc = cron.GetNextOccurrence(DateTime.UtcNow) ?? throw new InvalidOperationException($"Unable to get next occurance of {cron}");

                var jitter = TimeSpan.FromSeconds(random.Next(pipelineConfiguration.JitterSeconds));

                var delay = nextUtc - DateTime.UtcNow + jitter;

                _logger.LogInformation("Next occurrence of {Sources} is {NextUtc}, waiting {DelaySeconds}s ({JitterSeconds}s jitter)", string.Join(", ", sources.Select(x => x.ToString())), nextUtc, delay.TotalSeconds, jitter.TotalSeconds);

                await Task.Delay(delay, cancellation);

                try
                {
                    await _pipelineExecutor.RunPipeline(sources, extractor, tracker, enrichers, destinations, cancellation);
                }
                catch (Exception ex) when (!cancellation.IsCancellationRequested)
                {
                    _logger.LogCritical(ex, "Exception from pipeline");
                }
            }
            while (!cancellation.IsCancellationRequested);
        }
    }
}