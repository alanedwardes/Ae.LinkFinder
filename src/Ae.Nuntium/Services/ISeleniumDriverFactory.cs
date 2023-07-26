using OpenQA.Selenium;

namespace Ae.Nuntium.Services
{
    public interface ISeleniumDriverFactory
    {
        Task UseWebDriver(Func<IWebDriver, Task> drive, CancellationToken cancellation);
    }
}
