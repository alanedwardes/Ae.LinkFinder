using Ae.LinkFinder.Destinations;
using Ae.LinkFinder.Extractors;
using Ae.LinkFinder.Sources;
using Ae.LinkFinder.Trackers;
using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ae.LinkFinder
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Bootstrapping");

            var provider = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<Program>>();

            var rawConfiguration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.secret.json"), true)
                .Build();

            var configuration = GetConfiguration<LinkFinderConfiguration>(rawConfiguration);

            logger.LogInformation("Loaded configuration with {Finders} finders", configuration.Finders.Count);

            var tasks = new List<Task>();

            foreach (var finder in configuration.Finders)
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

        private static IContentSource GetSource(LinkFinderType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "facebook":
                    return ActivatorUtilities.CreateInstance<FacebookGroupPostSource>(serviceProvider, GetConfiguration<FacebookGroupPostSource.Configuration>(type.Configuration));
                case "twitter":
                    return ActivatorUtilities.CreateInstance<TwitterSource>(serviceProvider, GetConfiguration<TwitterSource.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IPostExtractor GetExtractor(LinkFinderType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "facebookhtml":
                    return ActivatorUtilities.CreateInstance<FacebookGroupHtmlPostExtractor>(serviceProvider);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static ILinkDestination GetDestination(LinkFinderType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "rocketchatwebhook":
                    return ActivatorUtilities.CreateInstance<RocketChatWebhookDestination>(serviceProvider, GetConfiguration<RocketChatWebhookDestination.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static ILinkTracker GetTracker(LinkFinderType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "file":
                    return ActivatorUtilities.CreateInstance<FileLinkTracker>(serviceProvider, GetConfiguration<FileLinkTracker.Configuration>(type.Configuration));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static async Task GetLinks(CronExpression cronExpression, IContentSource source, IPostExtractor extractor, ILinkTracker tracker, IList<ILinkDestination> destinations, CancellationToken token, IServiceProvider serviceProvider, bool testing)
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

                    var posts = await extractor.ExtractPosts(content);

                    var unseen = await tracker.GetUnseenLinks(posts.Select(x => x.Permalink), token);

                    serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Found {Unseen} unseen links of {Total} total", unseen.Count(), posts.Count);

                    foreach (var destination in destinations)
                    {
                        await destination.PostLinks(posts.Where(x => unseen.Contains(x.Permalink)));
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