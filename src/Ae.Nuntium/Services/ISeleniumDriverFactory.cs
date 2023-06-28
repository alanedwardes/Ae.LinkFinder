using OpenQA.Selenium;

namespace Ae.Nuntium.Services
{
    public interface ISeleniumDriverFactory
    {
        IWebDriver CreateWebDriver();
    }
}
