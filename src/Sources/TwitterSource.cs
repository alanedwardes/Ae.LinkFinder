using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace Ae.LinkFinder.Sources
{
    public sealed class TwitterSource : IContentSource
    {
        public sealed class Configuration
        {
            public Uri SeleniumAddress { get; set; }
            public Uri TwitterHandle { get; set; }
        }

        private readonly ILogger<TwitterSource> _logger;
        private readonly Configuration _configuration;

        public TwitterSource(ILogger<TwitterSource> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<SourceDocument> GetContent(CancellationToken token)
        {
            _logger.LogInformation("Loading {Address}", _configuration.TwitterHandle);

            var driver = new RemoteWebDriver(_configuration.SeleniumAddress, new ChromeOptions());

            var baseUri = "https://twitter.com/" + _configuration.TwitterHandle;

            driver.Navigate().GoToUrl(baseUri);

            Actions builder = new Actions(driver);

            TimeSpan RandomShortTimeSpan()
            {
                var random = new Random();
                return TimeSpan.FromSeconds(1 + random.NextDouble() * 3);
            }

            void PressKey(string key)
            {
                builder.Pause(RandomShortTimeSpan());

                builder.KeyDown(key);
                builder.KeyUp(key);
            }

            await Task.Delay(RandomShortTimeSpan(), token);

            // Scroll down the page to load a few more posts
            PressKey(Keys.End);
            PressKey(Keys.End);
            PressKey(Keys.End);
            PressKey(Keys.End);

            builder.Pause(RandomShortTimeSpan());

            _logger.LogInformation("Executing input");

            builder.Perform();

            //var regex = new Regex(_configuration.TwitterHandle + "\\/status\\/(?<id>[0-9]+)");

            _logger.LogInformation("Exporting page source");

            //var matches = regex.Matches(driver.PageSource);

            var sourceDocument = new SourceDocument
            {
                Body = driver.PageSource,
                Source = new Uri(driver.Url, UriKind.Absolute)
            };

            driver.Quit();

            return sourceDocument;

            //var links = matches.Select(x => x.Groups["id"].ToString()).Select(x => new Uri($"{baseUri}/status/{x}")).ToHashSet();

            //_logger.LogInformation("Found {Links} links out of {Matches} matches", links.Count, matches.Count);

            //return links;
        }
    }
}