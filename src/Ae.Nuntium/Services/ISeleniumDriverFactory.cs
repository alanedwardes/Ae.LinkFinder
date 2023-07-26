using OpenQA.Selenium;

namespace Ae.Nuntium.Services
{
    public interface ISeleniumDriverFactory
    {
        Task UseWebDriver(Action<IWebDriver> drive, CancellationToken cancellation);
    }
}
