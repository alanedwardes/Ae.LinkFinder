using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Ae.Nuntium.Services
{
    public sealed class GoogleChromeSeleniumDriverFactory : ISeleniumDriverFactory
    {
        public async Task UseWebDriver(Func<IWebDriver, Task> drive, CancellationToken cancellation)
        {
            var driver = new ChromeDriver();
            await drive(driver);
            driver.Quit();
        }
    }
}
