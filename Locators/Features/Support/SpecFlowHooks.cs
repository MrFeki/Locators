using Microsoft.Extensions.Logging;
using TechTalk.SpecFlow;

namespace Locators.Features.Support
{
    [Binding]
    public sealed class SpecFlowHooks
    {
        private readonly ScenarioContext _scenarioContext;

        public SpecFlowHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            TestContextHelper.Init();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            try
            {
                if (_scenarioContext.TestError != null)
                {
                    var screenshot = TestContextHelper.CaptureScreenshot(_scenarioContext.ScenarioInfo.Title);
                    TestContextHelper.Logger?.LogError(_scenarioContext.TestError, "Scenario failed. Screenshot: {Screenshot}", screenshot);
                }
            }
            finally
            {
                TestContextHelper.Cleanup();
            }
        }
    }
}
