using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Ae.Nuntium.Services
{
    public sealed class GoogleChromeSeleniumDriverFactory : ISeleniumDriverFactory
    {
        public Task UseWebDriver(Action<IWebDriver> drive, CancellationToken cancellation)
        {
            var driver = new ChromeDriver();
            drive(driver);
            driver.Quit();

            return Task.CompletedTask;
        }
    }
}
