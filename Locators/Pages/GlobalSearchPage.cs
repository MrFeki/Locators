using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class GlobalSearchPage : BasePage
    {
        public GlobalSearchPage(IWebDriver driver, WebDriverWait wait, ILogger logger)
            : base(driver, wait, logger)
        {
        }

        public void EnterGlobalSearchQuery(IWebElement searchInput, string query)
        {
            searchInput.Clear();
            searchInput.SendKeys(query);
            logger.LogInformation("Search query entered: {Query}", query);
        }

        public void ClickGlobalFindButton()
        {
            var findButton = WaitForVisibleAndEnabled(By.XPath("//span[normalize-space()='Find']/ancestor::button[contains(@class,'custom-search-button')]"));
            findButton.Click();
            logger.LogInformation("Find button clicked.");
        }

        public void WaitForGlobalSearchResults()
        {
            wait.Until(d => d.FindElements(By.CssSelector(".search-results__title-link")).Count > 0);
            logger.LogInformation("Global search results loaded.");
        }

        /// <summary>
        /// Returns the search result links as a list of text/href tuples. Page object does not assert; it only returns state.
        /// </summary>
        public System.Collections.Generic.List<(string Text, string Href)> GetSearchResultLinks()
        {
            var links = driver.FindElements(By.CssSelector(".search-results__title-link")).ToList();

            var linkData = links.Select(link => (Text: GetElementText(link), Href: link.GetAttribute("href") ?? string.Empty)).ToList();

            logger.LogInformation("Retrieved {Count} search result link(s).", linkData.Count);

            return linkData;
        }
    }
}
