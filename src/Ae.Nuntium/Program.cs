using Ae.Nuntium.Configuration;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ae.Nuntium
{
    public sealed class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Bootstrapping");

            MainConfiguration configuration = BuildConfig();

            ServiceProvider provider = BuildProvider(configuration);

            await RunPipelines(configuration, provider);
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
            var services = new ServiceCollection();

            services.AddHttpClient("GZIP_CLIENT")
                .AddHttpMessageHandler(() => new ExceptionDelegatingHandler())
                .AddHttpMessageHandler(() => new GlobalTimeoutDelegatingHandler())
                .ConfigurePrimaryHttpMessageHandler(_ => new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                })
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
                });

            var logLevel = configuration.Pipelines.Any(x => x.Testing) ? LogLevel.Debug : LogLevel.Warning;

            return services.AddHttpClient()
                .AddLogging(x => x.AddConsole().SetMinimumLevel(logLevel))
                .AddSingleton(x => x.GetRequiredService<IPipelineServiceFactory>().GetSeleniumDriver(configuration.SeleniumDriver))
                .AddSingleton<IPipelineExecutor, PipelineExecutor>()
                .AddSingleton<IPipelineServiceFactory, PipelineServiceFactory>()
                .AddSingleton<IPipelineScheduler, PipelineScheduler>()
                .AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>()
                .AddSingleton<IAmazonS3, AmazonS3Client>()
                .AddSingleton(x => TranslationClient.CreateFromApiKey(Environment.GetEnvironmentVariable("GOOGLE_TRANSLATE_API_KEY")))
                .BuildServiceProvider();
        }

        private static async Task RunPipelines(MainConfiguration configuration, IServiceProvider provider)
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

            try
            {
                await provider.GetRequiredService<IPipelineScheduler>().Schedule(configuration, cts.Token);
            }
            catch when (!cts.IsCancellationRequested)
            {
                throw;
            }
        }
    }
}