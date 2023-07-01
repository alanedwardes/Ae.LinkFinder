using Ae.Nuntium.Configuration;
using Cronos;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public sealed class NuntiumFinderScheduler : INuntiumFinderScheduler
    {
        private readonly ILogger<NuntiumFinderScheduler> _logger;
        private readonly INuntiumServiceFactory _nuntiumServiceFactory;
        private readonly INuntiumFinderRunner _nuntiumFinderRunner;

        public NuntiumFinderScheduler(ILogger<NuntiumFinderScheduler> logger, INuntiumServiceFactory nuntiumServiceFactory, INuntiumFinderRunner nuntiumFinderRunner)
        {
            _logger = logger;
            _nuntiumServiceFactory = nuntiumServiceFactory;
            _nuntiumFinderRunner = nuntiumFinderRunner;
        }

        public async Task Schedule(NuntiumConfiguration configuration, CancellationToken cancellation)
        {
            var tasks = new List<Task>();

            foreach (var finder in configuration.Finders)
            {
                var cron = CronExpression.Parse(finder.Cron);
                var source = _nuntiumServiceFactory.GetSource(finder.Source);
                var extractor = _nuntiumServiceFactory.GetExtractor(finder.Extractor);
                var tracker = _nuntiumServiceFactory.GetTracker(finder.Tracker);
                var destinations = finder.Destinations.Select(x => _nuntiumServiceFactory.GetDestination(x)).ToList();

                if (finder.Testing)
                {
                    _logger.LogInformation("Running {Source} in test mode", source);
                    await _nuntiumFinderRunner.RunOnce(source, extractor, tracker, destinations, cancellation);
                }
                else
                {
                    tasks.Add(_nuntiumFinderRunner.RunContinuously(cron, source, extractor, tracker, destinations, cancellation));
                }
            }

            _logger.LogInformation("Started {Tasks} tasks", tasks.Count);

            await Task.WhenAll(tasks);
        }
    }
}