using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;
using Locators.Features.Support;

namespace Locators.Features.Steps
{
    [Binding]
    public class ServicesSteps
    {
        private IWebDriver driver => TestContextHelper.Driver!;
        private WebDriverWait wait => TestContextHelper.Wait!;
        private ILogger logger => TestContextHelper.Logger!;

        private readonly By ServicesLink = By.CssSelector("a.top-navigation__item-link[href='/services']");
        private readonly By GenerativeAiLink = By.CssSelector("a.top-navigation__sub-link[href='/services/artificial-intelligence/generative-ai']");
        private readonly By ResponsibleAiLink = By.CssSelector("a.top-navigation__sub-link[href='/services/artificial-intelligence/responsible-ai']");



        [When("I click the \"Services\" link")]
        public void WhenIClickTheServicesLink()
        {
            var link = wait.Until(d => d.FindElements(ServicesLink).Count > 0 ? d.FindElement(ServicesLink) : null);
            Assert.IsNotNull(link, "Services link was not found.");

            // Hover over the Services link to reveal the dropdown menu instead of clicking it
            try
            {
                new Actions(driver).MoveToElement(link).Perform();
            }
            catch
            {
                // fallback to JavaScript hover if Actions fails
                ((IJavaScriptExecutor)driver).ExecuteScript(@"var evObj = document.createEvent('MouseEvents'); evObj.initEvent('mouseover', true, false); arguments[0].dispatchEvent(evObj);", link);
            }

            logger.LogInformation("Services link hovered.");
        }

        [When("I select the \"(.*)\" service from the dropdown")]
        public void WhenISelectTheServiceFromTheDropdown(string category)
        {
            // Wait for the category link to appear in the revealed dropdown
            IWebElement? categoryLink = wait.Until(d =>
            {
                try
                {
                    var byText = d.FindElements(By.LinkText(category)).Count > 0 ? d.FindElement(By.LinkText(category)) : null;
                    if (byText != null && byText.Displayed) return byText;

                    if (string.Equals(category, "Generative AI", StringComparison.OrdinalIgnoreCase))
                    {
                        var gen = d.FindElements(GenerativeAiLink).Count > 0 ? d.FindElement(GenerativeAiLink) : null;
                        if (gen != null && gen.Displayed) return gen;
                    }

                    if (string.Equals(category, "Responsible AI", StringComparison.OrdinalIgnoreCase))
                    {
                        var res = d.FindElements(ResponsibleAiLink).Count > 0 ? d.FindElement(ResponsibleAiLink) : null;
                        if (res != null && res.Displayed) return res;
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            });

            Assert.IsNotNull(categoryLink, $"Could not find category link for '{category}'.");

            TestContextHelper.ScrollToAndClick(categoryLink!);

            logger.LogInformation("Category link '{Category}' clicked.", category);
        }

        [Then("I should see a page title containing \"(.*)\"")]
        public void ThenIShouldSeeAPageTitleContaining(string expectedTitle)
        {
            // Wait for title or h1 to be present
            bool titleMatches = wait.Until(d =>
            {
                try
                {
                    string title = d.Title ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(title) && title.IndexOf(expectedTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;

                    var h1 = d.FindElements(By.TagName("h1")).Count > 0 ? d.FindElement(By.TagName("h1")) : null;
                    if (h1 != null)
                    {
                        string h1Text = h1.Text ?? string.Empty;
                        return h1Text.IndexOf(expectedTitle, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            });

            Assert.IsTrue(titleMatches, $"Expected page title or H1 to contain '{expectedTitle}'.");

            logger.LogInformation("Page title or H1 contains: {Expected}", expectedTitle);
        }

        [Then("the \"Our Related Expertise\" section should be visible")]
        public void ThenTheOurRelatedExpertiseSectionShouldBeVisible()
        {
            var locator = By.XPath("//section//*[contains(normalize-space(.), 'Our Related Expertise')]|//*[contains(normalize-space(.), 'Our Related Expertise')]");

            var element = wait.Until(d =>
            {
                try
                {
                    var elems = d.FindElements(locator);
                    var first = elems.Count > 0 ? elems[0] : null;
                    if (first != null && first.Displayed)
                        return first;
                    return null;
                }
                catch
                {
                    return null;
                }
            });

            Assert.IsNotNull(element, "'Our Related Expertise' section was not found or not visible.");

            logger.LogInformation("'Our Related Expertise' section is visible.");
        }
    }
}
