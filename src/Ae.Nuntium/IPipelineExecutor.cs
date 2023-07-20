using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;

namespace Ae.Nuntium
{
    public interface IPipelineExecutor
    {
        Task RunPipeline(IList<IContentSource> sources, IList<IPostExtractor> extractors, IPostTracker tracker, IList<IExtractedPostEnricher> enrichers, IList<IExtractedPostDestination> destinations, CancellationToken cancellation);
    }
}