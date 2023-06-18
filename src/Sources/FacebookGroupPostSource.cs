using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System.Text.RegularExpressions;

namespace Ae.LinkFinder.Sources
{
    public sealed class FacebookGroupPostSource : ILinkSource
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

        public async Task<ISet<Uri>> GetLinks(CancellationToken token)
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

            var regex = new Regex("(?<type>posts|permalink)\\\\?\\/(?<id>[0-9]+)");

            _logger.LogInformation("Exporting page source");

            var matches = regex.Matches(driver.PageSource);

            driver.Quit();

            var links = matches.Select(x => x.Groups["id"].ToString()).Select(x => new Uri($"{_configuration.GroupAddress}posts/{x}/")).ToHashSet();

            _logger.LogInformation("Found {Links} links out of {Matches} matches", links.Count, matches.Count);

            return links;
        }
    }
}