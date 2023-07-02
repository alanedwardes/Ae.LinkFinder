using Ae.Nuntium.Configuration;
using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Services;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;

namespace Ae.Nuntium
{
    public interface IConfigurationDrivenServiceFactory
    {
        IExtractedPostDestination GetDestination(ConfiguredType type);
        IPostExtractor GetExtractor(ConfiguredType type);
        ISeleniumDriverFactory GetSeleniumDriver(ConfiguredType type);
        IContentSource GetSource(ConfiguredType type);
        ILinkTracker GetTracker(ConfiguredType type);
        IExtractedPostEnricher GetEnricher(ConfiguredType type);
    }
}