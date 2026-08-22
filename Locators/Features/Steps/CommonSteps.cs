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
            // Navigation is performed in the BeforeScenario hook (TestContextHelper.Init)
            // This step simply ensures we have an initialized driver and marks the step as implemented.
            if (TestContextHelper.Driver == null)
                throw new InvalidOperationException("WebDriver was not initialized. Ensure SpecFlow hooks ran.");
        }
    }
}
