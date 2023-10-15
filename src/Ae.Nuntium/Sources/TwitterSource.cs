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

        public async Task<SourceDocument> GetContent(CancellationToken cancellation)
        {
            var sourceDocument = new SourceDocument();

            await _seleniumDriverFactory.UseWebDriver(async driver =>
            {
                _logger.LogInformation("Loading {Address}", _configuration.ProfileAddress);

                // Load the login page first, so that Chrome allows us to set cookies
                driver.Navigate().GoToUrl("https://twitter.com/i/flow/login");
                driver.Manage().Cookies.AddCookie(CreateAuthCookie());

                // Wait a little while
                await Task.Delay(RandomShortTimeSpan(), cancellation);

                driver.Navigate().GoToUrl(_configuration.ProfileAddress);

                Actions builder = new(driver);

                TimeSpan RandomShortTimeSpan()
                {
                    var random = new Random();
                    return TimeSpan.FromSeconds(1 + random.NextDouble() * 3);
                }

                builder.Pause(RandomShortTimeSpan());

                builder.Perform();

                _logger.LogInformation("Exporting page source");

                sourceDocument.Body = driver.PageSource;
                sourceDocument.Address = new Uri(driver.Url, UriKind.Absolute);
            }, cancellation);

            return sourceDocument;
        }

        private Cookie CreateAuthCookie()
        {
            return new Cookie("auth_token", _configuration.AuthToken, ".twitter.com", "/", DateTime.UtcNow.AddYears(1), true, true, "None");
        }

        public override string ToString() => _configuration.ProfileAddress.ToString();
    }
}