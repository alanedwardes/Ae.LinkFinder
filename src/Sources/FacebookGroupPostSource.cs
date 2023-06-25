using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace Ae.LinkFinder.Sources
{
    public sealed class FacebookGroupPostSource : IContentSource
    {
        public sealed class Configuration
        {
            public Uri SeleniumAddress { get; set; }
            public Uri GroupAddress { get; set; }
        }

        private readonly ILogger<FacebookGroupPostSource> _logger;
        private readonly Configuration _configuration;

        public FacebookGroupPostSource(ILogger<FacebookGroupPostSource> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<SourceDocument> GetContent(CancellationToken token)
        {
            _logger.LogInformation("Loading {Address}", _configuration.GroupAddress);

            var driver = new RemoteWebDriver(_configuration.SeleniumAddress, new ChromeOptions());

            driver.Navigate().GoToUrl(_configuration.GroupAddress);

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

            // Accept cookies
            builder.KeyDown(Keys.Shift);
            PressKey(Keys.Tab);
            builder.KeyUp(Keys.Shift);
            PressKey(Keys.Enter);

            // Scroll down the page to load a few more posts
            PressKey(Keys.End);
            PressKey(Keys.End);
            PressKey(Keys.End);
            PressKey(Keys.End);

            builder.Pause(RandomShortTimeSpan());

            _logger.LogInformation("Executing input");

            builder.Perform();

            _logger.LogInformation("Exporting page source");

            var sourceDocument = new SourceDocument
            {
                Body = driver.PageSource,
                Source = new Uri(driver.Url, UriKind.Absolute)
            };

            driver.Quit();

            return sourceDocument;
        }
    }
}