using System;
using TechTalk.SpecFlow;
using Locators.Features.Support;

namespace Locators.Features.Support
{
    [Binding]
    public sealed class SpecFlowHooks
    {
        [BeforeScenario]
        public void BeforeScenario()
        {
            TestContextHelper.Init();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            TestContextHelper.Cleanup();
        }
    }
}
