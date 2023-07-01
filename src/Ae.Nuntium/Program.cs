using Ae.Nuntium.Destinations;
using Ae.Nuntium.Extractors;
using Ae.Nuntium.Services;
using Ae.Nuntium.Sources;
using Ae.Nuntium.Trackers;
using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Bootstrapping");

            var rawConfiguration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.secret.json"), true)
                .Build();

            var configuration = GetConfiguration<MainConfiguration>(rawConfiguration);

            var provider = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddHttpClient()
                .AddSingleton(x => GetSeleniumDriver(configuration.SeleniumDriver, x))
                .BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Loaded configuration with {Finders} finders", configuration.Finders.Count);

            var tasks = new List<Task>();

            var testingFinders = configuration.Finders.Where(x => x.Testing);

            foreach (var finder in testingFinders.Any() ? testingFinders : configuration.Finders)
            {
                var cron = CronExpression.Parse(finder.Cron);
                var source = GetSource(finder.Source, provider);
                var extractor = GetExtractor(finder.Extractor, provider);
                var tracker = GetTracker(finder.Tracker, provider);
                var destinations = finder.Destinations.Select(x => GetDestination(x, provider)).ToList();

                tasks.Add(GetLinks(cron, source, extractor, tracker, destinations, CancellationToken.None, provider, finder.Testing));
            }

            logger.LogInformation("Started {Tasks} tasks", tasks.Count);

            await Task.WhenAll(tasks);

            logger.LogInformation("Exiting");
        }

        private static TConfiguration GetConfiguration<TConfiguration>(IConfiguration rawConfiguration) where TConfiguration : new()
        {
            var configuration = new TConfiguration();
            rawConfiguration.Bind(configuration);
            return configuration;
        }

        private static ISeleniumDriverFactory GetSeleniumDriver(ConfiguredType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "Remote":
                    return ActivatorUtilities.CreateInstance<RemoteSeleniumDriverFactory>(serviceProvider, GetConfiguration<RemoteSeleniumDriverFactory.Configuration>(type.Configuration));
                case "GoogleChrome":
                    return ActivatorUtilities.CreateInstance<GoogleChromeSeleniumDriverFactory>(serviceProvider);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IContentSource GetSource(ConfiguredType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "Facebook":
                    return ActivatorUtilities.CreateInstance<FacebookGroupSource>(serviceProvider, GetConfiguration<FacebookGroupSource.Configuration>(type.Configuration));
                case "Twitter":
                    return ActivatorUtilities.CreateInstance<TwitterSource>(serviceProvider, GetConfiguration<TwitterSource.Configuration>(type.Configuration));
                case "Http":
                    return ActivatorUtilities.CreateInstance<HttpSource>(serviceProvider, GetConfiguration<HttpSource.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IPostExtractor GetExtractor(ConfiguredType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "FacebookHtml":
                    return ActivatorUtilities.CreateInstance<FacebookGroupHtmlExtractor>(serviceProvider);
                case "TwitterHtml":
                    return ActivatorUtilities.CreateInstance<TwitterHtmlExtractor>(serviceProvider);
                case "Rss":
                    return ActivatorUtilities.CreateInstance<RssExtractor>(serviceProvider);
                case "Regex":
                    return ActivatorUtilities.CreateInstance<RegexExtractor>(serviceProvider, GetConfiguration<RegexExtractor.Configuration>(type.Configuration));
                case "Json":
                    return ActivatorUtilities.CreateInstance<JsonExtractor>(serviceProvider, GetConfiguration<JsonExtractor.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IExtractedPostDestination GetDestination(ConfiguredType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "RocketChatWebhook":
                    return ActivatorUtilities.CreateInstance<RocketChatWebhookDestination>(serviceProvider, GetConfiguration<RocketChatWebhookDestination.Configuration>(type.Configuration));
                case "DiscordWebhook":
                    return ActivatorUtilities.CreateInstance<DiscordWebhookDestination>(serviceProvider, GetConfiguration<DiscordWebhookDestination.Configuration>(type.Configuration));
                case "Console":
                    return ActivatorUtilities.CreateInstance<ConsoleDestination>(serviceProvider);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static ILinkTracker GetTracker(ConfiguredType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "File":
                    return ActivatorUtilities.CreateInstance<FileLinkTracker>(serviceProvider, GetConfiguration<FileLinkTracker.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static async Task GetLinks(CronExpression cronExpression, IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<IExtractedPostDestination> destinations, CancellationToken token, IServiceProvider serviceProvider, bool testing)
        {
            do
            {
                DateTime nextUtc = cronExpression.GetNextOccurrence(DateTime.UtcNow) ?? throw new InvalidOperationException($"Unable to get next occurance of {cronExpression}");

                var delay = nextUtc - DateTime.UtcNow;

                serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Next occurance is {NextUtc}, waiting {Delay}", nextUtc, delay);

                if (!testing)
                {
                    await Task.Delay(delay, token);
                }

                try
                {
                    serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Getting links with source {Source}", source);

                    var content = await source.GetContent(token);

                    var posts = (await extractor.ExtractPosts(content)).Where(x => x.HasContent()).ToList();

                    if (posts.Count == 0)
                    {
                        serviceProvider.GetRequiredService<ILogger<Program>>().LogWarning("Found no posts from source: {Source}", source);
                        continue;
                    }

                    var unseen = await tracker.GetUnseenLinks(posts.Select(x => x.Permalink), token);

                    serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Found {Unseen} unseen links of {Total} total", unseen.Count(), posts.Count);

                    foreach (var destination in destinations)
                    {
                        await destination.ShareExtractedPosts(posts.Where(x => unseen.Contains(x.Permalink)));
                    }

                    await tracker.SetLinksSeen(unseen, token);
                }
                catch (Exception ex)
                {
                    serviceProvider.GetRequiredService<ILogger<Program>>().LogCritical(ex, "Exception from finder");
                }
            }
            while (!testing);
        }
    }
}