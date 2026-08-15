using Microsoft.Extensions.Logging;
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
            var findButton = WaitForVisibleAndEnabled(By.XPath("//span[normalize-space()='Find']/ancestor::button[contains(@class,'custom-search-button')]") );
            findButton.Click();
            logger.LogInformation("Find button clicked.");
        }

        public void WaitForGlobalSearchResults()
        {
            wait.Until(d => d.FindElements(By.CssSelector(".search-results__title-link")).Count > 0);
            logger.LogInformation("Global search results loaded.");
        }

        public void ValidateAllLinksContainWord(string expectedWord)
        {
            var terms = (expectedWord ?? string.Empty)
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().Trim('"', '“', '”', '\'', '‘', '’'))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            Assert.That(terms, Is.Not.Empty, "No expected terms were provided for validation.");

            var links = driver.FindElements(By.CssSelector(".search-results__title-link"));
            Assert.That(links.Count, Is.GreaterThan(0), $"No search results were found for '{expectedWord}'.");

            var linkData = links.Select(link => new
            {
                Text = GetElementText(link),
                Href = link.GetAttribute("href") ?? string.Empty
            }).ToList();

            int matchCount = linkData.Count(link => terms.Any(term => link.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || link.Href.Contains(term, StringComparison.OrdinalIgnoreCase)));

            Assert.That(matchCount, Is.GreaterThan(0), $"No search result matched '{expectedWord}'.");

            logger.LogInformation("{MatchCount} result(s) matched {ExpectedWord}.", matchCount, expectedWord);

            var termMatches = terms.ToDictionary(term => term, term => linkData.Where(link => link.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || link.Href.Contains(term, StringComparison.OrdinalIgnoreCase)).Select(link => link.Text).ToList());

            foreach (var link in linkData)
            {
                var matchedTerms = terms.Where(term => link.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || link.Href.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matchedTerms.Count > 0)
                {
                    logger.LogInformation("MATCH -> Text: '{Text}', href: '{Href}', Terms: {Terms}", link.Text, link.Href, string.Join(", ", matchedTerms));
                }
                else
                {
                    logger.LogWarning("NO MATCH -> Text: '{Text}', href: '{Href}'", link.Text, link.Href);
                }
            }

            var missingTerms = termMatches.Where(r => r.Value.Count == 0).Select(r => r.Key).ToList();

            Assert.That(missingTerms, Is.Empty, $"No links found containing the expected term(s): {string.Join(", ", missingTerms)}.\nAll terms attempted: {string.Join(", ", terms)}.\nLinks observed: {string.Join(" | ", linkData.Select(l => l.Text))}");

            var invalidLinks = linkData.Where(link => !terms.Any(term => link.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || link.Href.Contains(term, StringComparison.OrdinalIgnoreCase))).Select(link => link.Text).ToList();

            if (invalidLinks.Count > 0)
            {
                logger.LogInformation("{Count} link(s) did not match expected term(s): {Examples}", invalidLinks.Count, string.Join(", ", invalidLinks.Take(5)));
            }
        }
    }
}
