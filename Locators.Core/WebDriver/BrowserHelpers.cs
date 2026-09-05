using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Core.WebDriver
{
    public static class BrowserHelpers
    {
        public static void ScrollIntoView(IWebDriver driver, IWebElement element)
        {
            if (driver is null) throw new ArgumentNullException(nameof(driver));
            if (element is null) throw new ArgumentNullException(nameof(element));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
        }

        public static void SafeClick(IWebDriver driver, IWebElement element)
        {
            if (driver is null) throw new ArgumentNullException(nameof(driver));
            if (element is null) throw new ArgumentNullException(nameof(element));

            try
            {
                element.Click();
            }
            catch (ElementClickInterceptedException)
            {
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("SafeClick failed to click the element.", ex);
                }
            }
        }

        public static bool TryAcceptCookieBanner(IWebDriver driver)
        {
            if (driver is null) return false;

            var buttons = driver.FindElements(By.Id("onetrust-accept-btn-handler"));
            IWebElement? acceptButton = buttons.FirstOrDefault(b => b.Displayed && b.Enabled);

            if (acceptButton is null) return false;

            try
            {
                acceptButton.Click();
                return true;
            }
            catch
            {
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", acceptButton);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool WaitForFileDownload(IWebDriver driver, string downloadDirectory, string expectedFilePath, int timeoutSeconds)
        {
            if (driver is null) throw new ArgumentNullException(nameof(driver));
            if (string.IsNullOrWhiteSpace(downloadDirectory)) throw new ArgumentNullException(nameof(downloadDirectory));
            if (string.IsNullOrWhiteSpace(expectedFilePath)) throw new ArgumentNullException(nameof(expectedFilePath));

            var wait = WaitFactory.Create(driver, timeoutSeconds);

            bool downloaded = wait.Until(d =>
            {
                bool expectedFileExists = File.Exists(expectedFilePath);
                bool temporaryDownloadExists = Directory.Exists(downloadDirectory) && Directory.GetFiles(downloadDirectory, "*.crdownload").Any();

                if (!expectedFileExists || temporaryDownloadExists) return false;

                long fileSize = new FileInfo(expectedFilePath).Length;
                return fileSize > 0;
            });

            return downloaded;
        }
    }
}
