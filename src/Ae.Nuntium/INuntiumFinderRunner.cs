using Ae.Nuntium.Destinations;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Cronos;

namespace Ae.Nuntium
{
    public interface INuntiumFinderRunner
    {
        Task RunOnce(IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<IExtractedPostDestination> destinations, CancellationToken cancellation);
        Task RunContinuously(CronExpression cronExpression, IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<IExtractedPostDestination> destinations, CancellationToken token);
    }
}