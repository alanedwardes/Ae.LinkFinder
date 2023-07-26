using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Ae.Nuntium.Services
{
    public sealed class RemoteSeleniumDriverFactory : ISeleniumDriverFactory
    {
        private readonly Configuration _configuration;
        private readonly SemaphoreSlim _semaphoreSlim;

        public sealed class Configuration
        {
            public Uri Address { get; set; }
            public int Concurrency { get; set; } = 1;
        }

        public RemoteSeleniumDriverFactory(Configuration configuration)
        {
            _configuration = configuration;
            _semaphoreSlim = new SemaphoreSlim(configuration.Concurrency, configuration.Concurrency);
        }

        public async Task UseWebDriver(Action<IWebDriver> drive, CancellationToken cancellation)
        {
            await _semaphoreSlim.WaitAsync(cancellation);

            try
            {
                IWebDriver driver = new RemoteWebDriver(_configuration.Address, new ChromeOptions());

                try
                {
                    drive(driver);
                }
                finally
                {
                    driver.Quit();
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
