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
    [Scope(Feature = "Careers search")]
    public class CareersSteps
    {
        private IWebDriver driver => TestContextHelper.Driver!;
        private WebDriverWait wait => TestContextHelper.Wait!;
        private ILogger logger => TestContextHelper.Logger!;

        private HomePage home => new HomePage(driver, wait, logger);
        private CareersPage careers => new CareersPage(driver, wait, logger);



        [When("I navigate to the Careers page")]
        public void WhenINavigateToTheCareersPage()
        {
            home.ClickCareers();
        }

        [When("I start the job search")]
        public void WhenIStartTheJobSearch()
        {
            careers.ClickStartYourSearch();
        }

        [When("I search for \"(.*)\"")]
        public void WhenISearchFor(string language)
        {
            careers.SearchForProgrammingLanguage(language);
        }

        [When("I choose country \"(.*)\"")]
        public void WhenIChooseCountry(string country)
        {
            careers.ChooseCountry(country);
        }

        [When("I set the Remote checkbox")]
        public void WhenISetTheRemoteCheckbox()
        {
            careers.SetCheckboxRemote();
        }

        [When("I submit the search")]
        public void WhenISubmitTheSearch()
        {
            careers.ClickSubmitButton();
            careers.WaitForJobResults();
        }

        [Then("the latest job should contain \"(.*)\"")]
        public void ThenTheLatestJobShouldContain(string language)
        {
            careers.ValidateLatestJobContainsLanguage(language);
        }
    }
}
