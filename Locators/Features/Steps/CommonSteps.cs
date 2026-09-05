using System;
using TechTalk.SpecFlow;
using Locators.Features.Support;

namespace Locators.Features.Steps
{
    [Binding]
    public class CommonSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public CommonSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I open the website homepage")]
        [Given(@"I open the EPAM homepage")]
        public void GivenIOpenTheWebsiteHomepage()
        {
            if (TestContextHelper.Driver == null)
                throw new InvalidOperationException("WebDriver was not initialized. Ensure SpecFlow hooks ran.");

            if (string.IsNullOrWhiteSpace(TestContextHelper.BaseUrl))
                throw new InvalidOperationException("BaseUrl is not configured. Set TestContextHelper.BaseUrl to the application's homepage URL.");

            try
            {
                TestContextHelper.Driver.Navigate().GoToUrl(TestContextHelper.BaseUrl);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to navigate to the application homepage '{TestContextHelper.BaseUrl}'.", ex);
            }
        }
    }
}
