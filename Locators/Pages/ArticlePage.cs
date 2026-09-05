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

        /// <summary>
        /// Reads the article page main heading (H1..H3) and returns it, or null if not found within the timeout.
        /// This method does not perform assertions; callers should verify the returned value in steps.
        /// </summary>
        public string? GetArticlePageTitle(TimeSpan? timeout = null)
        {
            logger.LogInformation("Reading article page title.");

            var articleWait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(15));

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
                catch
                {
                    return null;
                }
            });

            logger.LogInformation("Article page title read: {Title}", pageTitle);
            return pageTitle;
        }
    }
}
