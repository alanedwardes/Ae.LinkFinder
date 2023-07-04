using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Cronos;

namespace Ae.Nuntium
{
    public interface IPipelineExecutor
    {
        Task RunPipeline(IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IExtractedPostEnricher? enricher, IList<IExtractedPostDestination> destinations, CancellationToken cancellation);
    }
}