using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Ae.Nuntium.Services
{
    public sealed class RemoteSeleniumDriverFactory : ISeleniumDriverFactory
    {
        private readonly ILogger<RemoteSeleniumDriverFactory> _logger;
        private readonly Configuration _configuration;
        private readonly SemaphoreSlim _semaphoreSlim;

        public sealed class Configuration
        {
            public Uri Address { get; set; }
            public int Concurrency { get; set; } = 1;
        }

        public RemoteSeleniumDriverFactory(ILogger<RemoteSeleniumDriverFactory> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _semaphoreSlim = new SemaphoreSlim(configuration.Concurrency, configuration.Concurrency);
            _logger.LogInformation("Constructed RemoteSeleniumDriverFactory");
        }

        public async Task UseWebDriver(Func<IWebDriver, Task> drive, CancellationToken cancellation)
        {
            await _semaphoreSlim.WaitAsync(cancellation);

            try
            {
                IWebDriver driver = new RemoteWebDriver(_configuration.Address, new ChromeOptions());
                _logger.LogInformation("Constructed RemoteWebDriver");

                try
                {
                    await drive(driver);
                    _logger.LogInformation("Finished driving RemoteWebDriver");
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
