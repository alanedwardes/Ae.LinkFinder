using Ae.Nuntium.Configuration;
using Ae.Nuntium.Destinations;
using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Services;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Nuntium
{
    public sealed class PipelineServiceFactory : IPipelineServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PipelineServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private TConfiguration GetConfiguration<TConfiguration>(IConfiguration rawConfiguration) where TConfiguration : new()
        {
            var configuration = new TConfiguration();
            rawConfiguration.Bind(configuration);
            return configuration;
        }

        public ISeleniumDriverFactory GetSeleniumDriver(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "Remote":
                    return ActivatorUtilities.CreateInstance<RemoteSeleniumDriverFactory>(_serviceProvider, GetConfiguration<RemoteSeleniumDriverFactory.Configuration>(type.Configuration));
                case "GoogleChrome":
                    return ActivatorUtilities.CreateInstance<GoogleChromeSeleniumDriverFactory>(_serviceProvider);
                default:
                    throw new InvalidOperationException();
            }
        }

        public IContentSource GetSource(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "Facebook":
                    return ActivatorUtilities.CreateInstance<FacebookGroupSource>(_serviceProvider, GetConfiguration<FacebookGroupSource.Configuration>(type.Configuration));
                case "Twitter":
                    return ActivatorUtilities.CreateInstance<TwitterSource>(_serviceProvider, GetConfiguration<TwitterSource.Configuration>(type.Configuration));
                case "Http":
                    return ActivatorUtilities.CreateInstance<HttpSource>(_serviceProvider, GetConfiguration<HttpSource.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        public IPostExtractor GetExtractor(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "FacebookHtml":
                    return ActivatorUtilities.CreateInstance<FacebookGroupHtmlExtractor>(_serviceProvider);
                case "TwitterHtml":
                    return ActivatorUtilities.CreateInstance<TwitterHtmlExtractor>(_serviceProvider);
                case "Rss":
                    return ActivatorUtilities.CreateInstance<RssExtractor>(_serviceProvider);
                case "Regex":
                    return ActivatorUtilities.CreateInstance<RegexExtractor>(_serviceProvider, GetConfiguration<RegexExtractor.Configuration>(type.Configuration));
                case "Json":
                    return ActivatorUtilities.CreateInstance<JsonExtractor>(_serviceProvider, GetConfiguration<JsonExtractor.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        public IExtractedPostDestination GetDestination(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "RocketChatWebhook":
                    return ActivatorUtilities.CreateInstance<RocketChatWebhookDestination>(_serviceProvider, GetConfiguration<RocketChatWebhookDestination.Configuration>(type.Configuration));
                case "DiscordWebhook":
                    return ActivatorUtilities.CreateInstance<DiscordWebhookDestination>(_serviceProvider, GetConfiguration<DiscordWebhookDestination.Configuration>(type.Configuration));
                case "Console":
                    return ActivatorUtilities.CreateInstance<ConsoleDestination>(_serviceProvider);
                default:
                    throw new InvalidOperationException();
            }
        }

        public IPostTracker GetTracker(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "File":
                    return ActivatorUtilities.CreateInstance<FilePostTracker>(_serviceProvider, GetConfiguration<FilePostTracker.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        public IExtractedPostEnricher GetEnricher(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "SmartReader":
                    return ActivatorUtilities.CreateInstance<SmartReaderArticleEnricher>(_serviceProvider);
                case "WhitespaceRemoval":
                    return ActivatorUtilities.CreateInstance<WhitespaceRemovalEnricher>(_serviceProvider);
                case "HomepageMetadata":
                    return ActivatorUtilities.CreateInstance<HomepageMetadataEnricher>(_serviceProvider);
                case "AmazonS3MediaCache":
                    return ActivatorUtilities.CreateInstance<AmazonS3MediaCacheEnricher>(_serviceProvider, GetConfiguration<AmazonS3MediaCacheEnricher.Configuration>(type.Configuration));
                case "GoogleTranslate":
                    return ActivatorUtilities.CreateInstance<GoogleTranslateEnricher>(_serviceProvider, GetConfiguration<GoogleTranslateEnricher.Configuration>(type.Configuration));
                case "Filter":
                    return ActivatorUtilities.CreateInstance<FilterEnricher>(_serviceProvider, GetConfiguration<FilterEnricher.Configuration>(type.Configuration));
                case "Markdown":
                    return ActivatorUtilities.CreateInstance<MarkdownEnricher>(_serviceProvider);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
