using Ae.Nuntium.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Ae.Nuntium.Sources
{
    public sealed class TwitterSource : IContentSource
    {
        public sealed class Configuration
        {
            public Uri ProfileAddress { get; set; }
            public string AuthToken { get; set; }
        }

        private readonly ILogger<TwitterSource> _logger;
        private readonly ISeleniumDriverFactory _seleniumDriverFactory;
        private readonly Configuration _configuration;

        public TwitterSource(ILogger<TwitterSource> logger, ISeleniumDriverFactory seleniumDriverFactory, Configuration configuration)
        {
            _logger = logger;
            _seleniumDriverFactory = seleniumDriverFactory;
            _configuration = configuration;
        }

        public async Task<SourceDocument> GetContent(CancellationToken token)
        {
            _logger.LogInformation("Loading {Address}", _configuration.ProfileAddress);

            IWebDriver driver = _seleniumDriverFactory.CreateWebDriver();

            if (_configuration.AuthToken != null)
            {
                // Load the login page first, so that Chrome allows us to set cookies
                driver.Navigate().GoToUrl("https://twitter.com/i/flow/login");
                driver.Manage().Cookies.AddCookie(CreateAuthCookie());

                // Wait a little while
                await Task.Delay(RandomShortTimeSpan(), token);
            }

            driver.Navigate().GoToUrl(_configuration.ProfileAddress);

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

            await Task.Delay(RandomShortTimeSpan(), token);

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

        private Cookie CreateAuthCookie()
        {
            return new Cookie("auth_token", _configuration.AuthToken, ".twitter.com", "/", DateTime.UtcNow.AddYears(1), true, true, "None");
        }

        public override string ToString() => _configuration.ProfileAddress.ToString();
    }
}