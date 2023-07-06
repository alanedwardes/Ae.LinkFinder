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
                var source = _serviceFactory.GetSource(pipeline.Source ?? new());
                var extractor = _serviceFactory.GetExtractor(pipeline.Extractor ?? new());
                var tracker = _serviceFactory.GetTracker(pipeline.Tracker ?? new());
                var enrichers = pipeline.EnrichersMarshaled.Select(x => _serviceFactory.GetEnricher(x)).ToList();
                var destinations = pipeline.DestinationsMarshaled.Select(x => _serviceFactory.GetDestination(x)).ToList();

                if (pipeline.Testing)
                {
                    _logger.LogInformation("Running {Source} in test mode", source);
                    await _pipelineExecutor.RunPipeline(source, extractor, tracker, enrichers, destinations, cancellation);
                }
                else
                {
                    tasks.Add(RunContinuously(pipeline, source, extractor, tracker, enrichers, destinations, cancellation));
                }
            }

            _logger.LogInformation("Started {Tasks} tasks", tasks.Count);

            await Task.WhenAll(tasks);
        }

        public async Task RunContinuously(PipelineConfiguration pipelineConfiguration, IContentSource source, IPostExtractor extractor, IPostTracker tracker, IList<IExtractedPostEnricher> enrichers, IList<IExtractedPostDestination> destinations, CancellationToken cancellation)
        {
            var cron = CronExpression.Parse(pipelineConfiguration.Cron);
            var random = new Random();

            do
            {
                DateTime nextUtc = cron.GetNextOccurrence(DateTime.UtcNow) ?? throw new InvalidOperationException($"Unable to get next occurance of {cron}");

                var jitter = TimeSpan.FromSeconds(random.Next(pipelineConfiguration.JitterSeconds));

                var delay = nextUtc - DateTime.UtcNow + jitter;

                _logger.LogInformation("Next occurrence of {Source} is {NextUtc}, waiting {DelaySeconds}s ({JitterSeconds}s jitter)", source, nextUtc, delay.TotalSeconds, jitter.TotalSeconds);

                await Task.Delay(delay, cancellation);

                try
                {
                    await _pipelineExecutor.RunPipeline(source, extractor, tracker, enrichers, destinations, cancellation);
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