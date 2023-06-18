﻿using Ae.LinkFinder.Destinations;
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
                var tracker = GetTracker(finder.Tracker, provider);
                var destinations = finder.Destinations.Select(x => GetDestination(x, provider)).ToList();

                tasks.Add(GetLinks(cron, source, tracker, destinations, CancellationToken.None, provider));
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

        private static ILinkSource GetSource(LinkFinderType type, IServiceProvider serviceProvider)
        {
            switch (type.Type)
            {
                case "facebook":
                    return ActivatorUtilities.CreateInstance<FacebookGroupPostSource>(serviceProvider, GetConfiguration<FacebookGroupPostSource.Configuration>(type.Configuration));
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

        private static async Task GetLinks(CronExpression cronExpression, ILinkSource source, ILinkTracker tracker, IList<ILinkDestination> destinations, CancellationToken token, IServiceProvider serviceProvider)
        {
            while (true)
            {
                try
                {
                    serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Getting links with source {Source}", source);

                    var links = await source.GetLinks(token);

                    var unseen = await tracker.GetUnseenLinks(links, token);

                    serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Found {Unseen} unseen links of {Total} total", unseen.Count, links.Count);

                    foreach (var destination in destinations)
                    {
                        await destination.PostLinks(unseen);
                    }

                    await tracker.SetLinksSeen(unseen, token);
                }
                catch (Exception ex)
                {
                    serviceProvider.GetRequiredService<ILogger<Program>>().LogCritical(ex, "Exception from finder");
                }

                DateTime nextUtc = cronExpression.GetNextOccurrence(DateTime.UtcNow) ?? throw new InvalidOperationException($"Unable to get next occurance of {cronExpression}");
                await Task.Delay(nextUtc - DateTime.UtcNow, token);
            }
        }
    }
}