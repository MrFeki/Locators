using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class ServicesPage : BasePage
    {
        private readonly By ServicesLink = By.CssSelector("a.top-navigation__item-link[href='/services']");
        private readonly By GenerativeAiLink = By.CssSelector("a.top-navigation__sub-link[href='/services/artificial-intelligence/generative-ai']");
        private readonly By ResponsibleAiLink = By.CssSelector("a.top-navigation__sub-link[href='/services/artificial-intelligence/responsible-ai']");

        public ServicesPage(IWebDriver driver, WebDriverWait wait, ILogger logger)
            : base(driver, wait, logger)
        {
        }

        public void HoverServicesLink()
        {
            logger.LogInformation("Finding Services link and hovering.");
            var link = wait.Until(d =>
            {
                try
                {
                    return d.FindElements(ServicesLink).Count > 0 ? d.FindElement(ServicesLink) : null;
                }
                catch
                {
                    return null;
                }
            });

            if (link == null) throw new InvalidOperationException("Services link was not found.");

            try
            {
                MoveToElement(link);
            }
            catch
            {
                // fallback to JS hover
                ((IJavaScriptExecutor)driver).ExecuteScript(@"var evObj = document.createEvent('MouseEvents'); evObj.initEvent('mouseover', true, false); arguments[0].dispatchEvent(evObj);", link);
            }

            logger.LogInformation("Services link hovered.");
        }

        public void ClickServicesLink()
        {
            logger.LogInformation("Finding Services link and clicking.");
            var link = wait.Until(d =>
            {
                try
                {
                    return d.FindElements(ServicesLink).Count > 0 ? d.FindElement(ServicesLink) : null;
                }
                catch
                {
                    return null;
                }
            });

            if (link == null) throw new InvalidOperationException("Services link was not found.");

            try
            {
                ScrollToElement(link);
            }
            catch { }

            link.Click();

            logger.LogInformation("Services link clicked.");
        }

        public void SelectServiceFromDropdown(string category)
        {
            logger.LogInformation("Selecting service from dropdown: {Category}", category);

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

            if (categoryLink == null) throw new InvalidOperationException($"Could not find category link for '{category}'.");

            try
            {
                ScrollToElement(categoryLink);
            }
            catch { }

            categoryLink.Click();

            logger.LogInformation("Category link '{Category}' clicked.", category);
        }

        /// <summary>
        /// Returns the browser title as reported by the driver. This method does not assert.
        /// </summary>
        public string GetBrowserTitle()
        {
            try
            {
                return driver.Title ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Attempts to read the main heading (H1..H4) from the document. Returns null if no suitable heading is found within the timeout.
        /// This method intentionally does not assert; verification should happen in the step definitions.
        /// </summary>
        public string? GetMainHeading(TimeSpan? timeout = null)
        {
            logger.LogInformation("Attempting to read main heading.");

            var headingWait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(15));

            string? headingText = headingWait.Until(d =>
            {
                try
                {
                    IWebElement? heading = d.FindElements(By.XPath("//h1 | //h2 | //h3 | //h4")).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
                    if (heading is null) return null;
                    string text = GetElementText(heading);
                    return !string.IsNullOrWhiteSpace(text) ? text : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
                catch
                {
                    return null;
                }
            });

            logger.LogInformation("Main heading read: {Heading}", headingText);
            return headingText;
        }

        public bool IsOurRelatedExpertiseSectionVisible()
        {
            // Prefer locating a heading inside a section to avoid overly broad matches.
            var specificLocator = By.XPath(
                "//section[.//h1[normalize-space()='Our Related Expertise']" +
                " or .//h2[normalize-space()='Our Related Expertise']" +
                " or .//h3[normalize-space()='Our Related Expertise']" +
                " or .//h4[normalize-space()='Our Related Expertise']]");

            try
            {
                var element = wait.Until(d =>
                {
                    try
                    {
                        var elems = d.FindElements(specificLocator);
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

                if (element != null)
                {
                    logger.LogInformation("'Our Related Expertise' visible (specific locator): True");
                    return true;
                }
            }
            catch { }

            // Fallback: attempt a broader but still scoped search within sections only.
            var fallbackLocator = By.XPath("//section//*[contains(normalize-space(.), 'Our Related Expertise')]");
            var fallbackElement = wait.Until(d =>
            {
                try
                {
                    var elems = d.FindElements(fallbackLocator);
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

            bool visibleFallback = fallbackElement != null;
            logger.LogInformation("'Our Related Expertise' visible (fallback): {Visible}", visibleFallback);
            return visibleFallback;
        }
    }
}
