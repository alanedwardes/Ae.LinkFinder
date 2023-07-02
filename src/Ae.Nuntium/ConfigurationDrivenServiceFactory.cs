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
    public sealed class ConfigurationDrivenServiceFactory : IConfigurationDrivenServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ConfigurationDrivenServiceFactory(IServiceProvider serviceProvider)
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

        public ILinkTracker GetTracker(ConfiguredType type)
        {
            switch (type.Type)
            {
                case "File":
                    return ActivatorUtilities.CreateInstance<FileLinkTracker>(_serviceProvider, GetConfiguration<FileLinkTracker.Configuration>(type.Configuration));
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
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}