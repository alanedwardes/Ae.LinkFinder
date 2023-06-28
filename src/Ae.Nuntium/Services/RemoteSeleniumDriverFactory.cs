using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Ae.Nuntium.Services
{
    public sealed class RemoteSeleniumDriverFactory : ISeleniumDriverFactory
    {
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public Uri Address { get; set; }
        }

        public RemoteSeleniumDriverFactory(Configuration configuration)
        {
            _configuration = configuration;
        }

        public IWebDriver CreateWebDriver()
        {
            return new RemoteWebDriver(_configuration.Address, new ChromeOptions());
        }
    }
}
