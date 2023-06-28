using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Ae.Nuntium.Services
{
    public sealed class GoogleChromeSeleniumDriverFactory : ISeleniumDriverFactory
    {
        public IWebDriver CreateWebDriver()
        {
            return new ChromeDriver();
        }
    }
}
