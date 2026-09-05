using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class BasePage
    {
        protected readonly IWebDriver driver;
        protected readonly WebDriverWait wait;
        protected readonly ILogger logger;

        public BasePage(IWebDriver driver, WebDriverWait wait, ILogger logger)
        {
            this.driver = driver;
            this.wait = wait;
            this.logger = logger;
        }

        protected IWebElement WaitForVisibleAndEnabled(By locator)
        {
            return wait.Until(d =>
            {
                try
                {
                    var elements = d.FindElements(locator);
                    return elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            }) ?? throw new InvalidOperationException($"Element was not found: {locator}");
        }

        protected void ScrollToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
        }

        protected void MoveToElement(IWebElement element)
        {
            new OpenQA.Selenium.Interactions.Actions(driver).MoveToElement(element).Perform();
        }

        protected static string GetElementText(IWebElement element)
        {
            string text = element.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                text = element.GetAttribute("textContent") ?? element.GetAttribute("innerText") ?? string.Empty;
            }

            return NormalizeWhitespace(text);
        }

        protected static string NormalizeWhitespace(string text)
        {
            return string.Join(" ", text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
