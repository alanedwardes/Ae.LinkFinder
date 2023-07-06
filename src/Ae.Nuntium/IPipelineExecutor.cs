using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;

namespace Ae.Nuntium
{
    public interface IPipelineExecutor
    {
        Task RunPipeline(IContentSource source, IPostExtractor extractor, IPostTracker tracker, IList<IExtractedPostEnricher> enrichers, IList<IExtractedPostDestination> destinations, CancellationToken cancellation);
    }
}