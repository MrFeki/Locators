using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class ArticlePage : BasePage
    {
        public ArticlePage(IWebDriver driver, WebDriverWait wait, ILogger logger)
            : base(driver, wait, logger)
        {
        }

        public string GetArticlePageTitle()
        {
            logger.LogInformation("Waiting for article page title.");

            var articleWait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            string? pageTitle = articleWait.Until(d =>
            {
                try
                {
                    IWebElement? heading = d.FindElements(By.XPath("//h1 | //h2 | //h3")).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
                    if (heading is null) return null;
                    string text = GetElementText(heading);
                    return !string.IsNullOrWhiteSpace(text) ? text : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            Assert.That(pageTitle, Is.Not.Null.And.Not.Empty, "Could not determine article page title.");

            return pageTitle!;
        }
    }
}
