using Ae.Nuntium.Configuration;
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

            NuntiumConfiguration configuration = BuildConfig();

            ServiceProvider provider = BuildProvider(configuration);

            await RunFinders(configuration, provider);
        }

        private static NuntiumConfiguration BuildConfig()
        {
            var rawConfiguration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.secret.json"), true)
                .Build();

            var configuration = new NuntiumConfiguration();
            rawConfiguration.Bind(configuration);
            return configuration;
        }

        private static ServiceProvider BuildProvider(NuntiumConfiguration configuration)
        {
            return new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddHttpClient()
                .AddSingleton(x => x.GetRequiredService<INuntiumServiceFactory>().GetSeleniumDriver(configuration.SeleniumDriver))
                .AddSingleton<INuntiumFinderRunner, NuntiumFinderRunner>()
                .AddSingleton<INuntiumServiceFactory, NuntiumServiceFactory>()
                .AddSingleton<INuntiumFinderScheduler, NuntiumFinderScheduler>()
                .BuildServiceProvider();
        }

        private static async Task RunFinders(NuntiumConfiguration configuration, IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Loaded configuration with {Finders} finders", configuration.Finders.Count);

            var cts = new CancellationTokenSource();

            void HandleCancel()
            {
                logger.LogCritical("Exit event recieved, cancelling all tasks");
                cts.Cancel();
            }

            Console.CancelKeyPress += (s, e) => HandleCancel();
            AppDomain.CurrentDomain.ProcessExit += (e, s) => HandleCancel();

            try
            {
                await provider.GetRequiredService<INuntiumFinderScheduler>().Schedule(configuration, cts.Token);
            }
            catch when (!cts.IsCancellationRequested)
            {
                throw;
            }
        }
    }
}