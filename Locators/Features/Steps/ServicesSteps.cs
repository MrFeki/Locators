using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;
using Locators.Features.Support;
using Locators.Pages;

namespace Locators.Features.Steps
{
    [Binding]
    public class ServicesSteps
    {
        private IWebDriver driver => TestContextHelper.Driver!;
        private WebDriverWait wait => TestContextHelper.Wait!;
        private ILogger logger => TestContextHelper.Logger!;

        [When("I hover over the Services link")]
        public void WhenIClickTheServicesLink()
        {
            var page = new ServicesPage(driver, wait, logger);
            page.HoverServicesLink();
        }

        [When("I select the \"(.*)\" service from the dropdown")]
        public void WhenISelectTheServiceFromTheDropdown(string category)
        {
            var page = new ServicesPage(driver, wait, logger);
            page.SelectServiceFromDropdown(category);
        }

        [Then("I should see a page title containing \"(.*)\"")]
        public void ThenIShouldSeeAPageTitleContaining(string expectedTitle)
        {
            var page = new ServicesPage(driver, wait, logger);
            // Retrieve state from the page object and assert in the step.
            string browserTitle = page.GetBrowserTitle();
            string? mainHeading = page.GetMainHeading();

            bool browserMatches = !string.IsNullOrWhiteSpace(browserTitle) && browserTitle.IndexOf(expectedTitle, StringComparison.OrdinalIgnoreCase) >= 0;
            bool headingMatches = !string.IsNullOrWhiteSpace(mainHeading) && mainHeading!.IndexOf(expectedTitle, StringComparison.OrdinalIgnoreCase) >= 0;

            Assert.IsTrue(browserMatches || headingMatches, $"Expected page title or main heading to contain '{expectedTitle}'. BrowserTitle: '{browserTitle}', MainHeading: '{mainHeading ?? "<none>"}'");

            logger.LogInformation("Page title/heading verification result. BrowserMatch: {BrowserMatch}, HeadingMatch: {HeadingMatch}", browserMatches, headingMatches);
        }

        [Then("the \"Our Related Expertise\" section should be visible")]
        public void ThenTheOurRelatedExpertiseSectionShouldBeVisible()
        {
            var page = new ServicesPage(driver, wait, logger);
            bool visible = page.IsOurRelatedExpertiseSectionVisible();
            Assert.IsTrue(visible, "'Our Related Expertise' section was not found or not visible.");

            logger.LogInformation("'Our Related Expertise' section is visible.");
        }
    }
}
