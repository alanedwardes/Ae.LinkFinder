using Ae.Nuntium.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Ae.Nuntium.Sources
{
    public sealed class FacebookGroupSource : IContentSource
    {
        public sealed class Configuration
        {
            public Uri GroupAddress { get; set; }
        }

        private readonly ILogger<FacebookGroupSource> _logger;
        private readonly Configuration _configuration;
        private readonly ISeleniumDriverFactory _seleniumDriverFactory;

        public FacebookGroupSource(ILogger<FacebookGroupSource> logger, ISeleniumDriverFactory seleniumDriverFactory, Configuration configuration)
        {
            _logger = logger;
            _seleniumDriverFactory = seleniumDriverFactory;
            _configuration = configuration;
        }

        public async Task<SourceDocument> GetContent(CancellationToken cancellation)
        {
            _logger.LogInformation("Loading {Address}", _configuration.GroupAddress);

            var sourceDocument = new SourceDocument();

            await _seleniumDriverFactory.UseWebDriver(async driver =>
            {
                driver.Navigate().GoToUrl(_configuration.GroupAddress);

                Actions builder = new(driver);

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

                await Task.Delay(RandomShortTimeSpan(), cancellation);

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

                sourceDocument.Body = driver.PageSource;
                sourceDocument.Address = new Uri(driver.Url, UriKind.Absolute);
            }, cancellation);

            return sourceDocument;
        }

        public override string ToString() => _configuration.GroupAddress.ToString();
    }
}