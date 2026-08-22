using System;
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
            globalSearch.ValidateAllLinksContainWord(query);
        }
    }
}
