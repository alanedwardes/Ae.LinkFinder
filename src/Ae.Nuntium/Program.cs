using Ae.Nuntium.Configuration;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium
{
    public sealed class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Bootstrapping");

            MainConfiguration configuration = BuildConfig();

            ServiceProvider provider = BuildProvider(configuration);

            await Run(configuration, provider);
        }

        private static MainConfiguration BuildConfig()
        {
            var rawConfiguration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.secret.json"), true)
                .Build();

            var configuration = new MainConfiguration();
            rawConfiguration.Bind(configuration);
            return configuration;
        }

        private static ServiceProvider BuildProvider(MainConfiguration configuration)
        {
            return new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddHttpClient()
                .AddSingleton(x => x.GetRequiredService<IPipelineServiceFactory>().GetSeleniumDriver(configuration.SeleniumDriver))
                .AddSingleton<IPipelineExecutor, PipelineExecutor>()
                .AddSingleton<IPipelineServiceFactory, PipelineServiceFactory>()
                .AddSingleton<IPipelineScheduler, PipelineScheduler>()
                .AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>()
                .AddSingleton<IAmazonS3, AmazonS3Client>()
                .BuildServiceProvider();
        }

        private static async Task Run(MainConfiguration configuration, IServiceProvider provider)
        {
            var logger = provider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Loaded configuration with {PipelineCount} pipelines", configuration.Pipelines.Count);

            var cts = new CancellationTokenSource();

            void HandleCancel()
            {
                logger.LogCritical("Exit event recieved, cancelling all tasks");
                cts.Cancel();
            }

            Console.CancelKeyPress += (s, e) => HandleCancel();
            AppDomain.CurrentDomain.ProcessExit += (e, s) => HandleCancel();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddLogging(x => x.AddConsole());
                    services.AddSingleton(configuration);
                    // Delegate any services requried to ASP.NET from the main container
                    services.AddSingleton(provider.GetRequiredService<IPipelineServiceFactory>());
                })
                .Build();

            var pipelinesTask = RunWrapped(provider.GetRequiredService<IPipelineScheduler>().Schedule(configuration, cts.Token), cts);
            var serverTask = RunWrapped(host.RunAsync(cts.Token), cts);

            // Await one of the tasks to report an exit, crash etc
            await Task.WhenAny(pipelinesTask, serverTask);
        }

        private static async Task RunWrapped(Task task, CancellationTokenSource cts)
        {
            try
            {
                await task;
            }
            catch when (!cts.IsCancellationRequested)
            {
                throw;
            }
        }
    }
}