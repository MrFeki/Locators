using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Locators.Core.Configuration;

namespace Locators.Core.WebDriver
{
    public static class WaitFactory
    {
        public static WebDriverWait Create(IWebDriver driver, int? timeoutSeconds = null)
        {
            if (driver is null) throw new ArgumentNullException(nameof(driver));

            int seconds = timeoutSeconds ?? TestConfiguration.Instance.Settings.ExplicitWaitSeconds;
            return new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
        }
    }
}
