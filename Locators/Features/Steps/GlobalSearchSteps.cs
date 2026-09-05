using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;
using Locators.Features.Support;
using Locators.Pages;

namespace Locators.Features.Steps
{
    [Binding]
    [Scope(Feature = "Global search")]
    public class GlobalSearchSteps
    {
        private IWebDriver driver => TestContextHelper.Driver!;
        private WebDriverWait wait => TestContextHelper.Wait!;
        private ILogger logger => TestContextHelper.Logger!;

        private HomePage home => new HomePage(driver, wait, logger);
        private GlobalSearchPage globalSearch => new GlobalSearchPage(driver, wait, logger);

        [When("I open the global search input")]
        public void WhenIOpenTheGlobalSearchInput()
        {
            home.ClickSearchIcon();
        }

        [When("I search for \"(.*)\"")]
        public void WhenISearchFor(string query)
        {
            var input = home.GetGlobalSearchInput();
            globalSearch.EnterGlobalSearchQuery(input, query);
            globalSearch.ClickGlobalFindButton();
            globalSearch.WaitForGlobalSearchResults();
        }

        [Then("all search result links should contain \"(.*)\"")]
        public void ThenAllSearchResultLinksShouldContain(string query)
        {
            var links = globalSearch.GetSearchResultLinks();

            var terms = (query ?? string.Empty)
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().Trim('"', '“', '”', '\'', '‘', '’'))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            Assert.IsNotNull(terms);
            Assert.IsTrue(terms.Any(), "No expected terms were provided for validation.");

            Assert.IsNotNull(links);
            Assert.IsTrue(links.Count > 0, $"No search results were found for '{query}'.");

            int matchCount = links.Count(link => terms.Any(term => link.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || link.Href.Contains(term, StringComparison.OrdinalIgnoreCase)));

            Assert.IsTrue(matchCount > 0, $"No search result matched '{query}'.");

            var termMatches = terms.ToDictionary(term => term, term => links.Where(link => link.Text.Contains(term, StringComparison.OrdinalIgnoreCase) || link.Href.Contains(term, StringComparison.OrdinalIgnoreCase)).Select(link => link.Text).ToList());

            var missingTerms = termMatches.Where(r => r.Value.Count == 0).Select(r => r.Key).ToList();
            Assert.IsEmpty(missingTerms, $"No links found containing the expected term(s): {string.Join(", ", missingTerms)}. Links observed: {string.Join(" | ", links.Select(l => l.Text))}");
        }
    }
}
