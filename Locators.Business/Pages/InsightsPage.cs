using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class InsightsPage : BasePage
    {
        public InsightsPage(IWebDriver driver, WebDriverWait wait, ILogger logger)
            : base(driver, wait, logger)
        {
        }

        public void WaitForInsightsContent()
        {
            logger.LogInformation("Waiting for Insights content.");

            bool contentLoaded = wait.Until(d => d.FindElements(By.XPath("//body//h1 | //body//h2 | //body//h3")).Any(e => e.Displayed));

            if (!contentLoaded)
            {
                throw new InvalidOperationException("Insights content did not load.");
            }

            logger.LogInformation("Insights content loaded.");
        }

        public void SwipeCarousel(int numberOfSwipes)
        {
            if (numberOfSwipes < 2)
            {
                throw new ArgumentException("Carousel must be swiped at least twice.", nameof(numberOfSwipes));
            }

            logger.LogInformation("Swiping carousel {Count} times.", numberOfSwipes);

            By nextButtonLocator = By.XPath("//button[contains(@class,'next') or contains(@class,'slick-next') or contains(@class,'slider__right-arrow') or contains(@aria-label,'Next') or contains(@title,'Next')]");

            for (int i = 0; i < numberOfSwipes; i++)
            {
                string previousTitle = GetCurrentCarouselTitle();

                IWebElement? nextButton = driver.FindElements(nextButtonLocator).FirstOrDefault(b => b.Displayed && b.Enabled);

                if (nextButton is null)
                {
                    throw new InvalidOperationException("Carousel Next button was not found.");
                }

                ScrollToElement(nextButton!);

                Locators.Core.WebDriver.BrowserHelpers.SafeClick(driver, nextButton!);

                bool slideChanged = wait.Until(d =>
                {
                    try
                    {
                        string? current = TryGetCurrentCarouselTitle(d);
                        return !string.IsNullOrWhiteSpace(current) && !string.Equals(previousTitle, current, StringComparison.OrdinalIgnoreCase);
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                });

                if (!slideChanged)
                {
                    throw new InvalidOperationException($"Carousel did not change after swipe {i + 1}.");
                }

                logger.LogInformation("Carousel swipe {Current}/{Total} completed.", i + 1, numberOfSwipes);
            }

            logger.LogInformation("Carousel swiping completed.");
        }

        private string GetCurrentCarouselTitle()
        {
            string? title = wait.Until(d =>
            {
                try
                {
                    return TryGetCurrentCarouselTitle(d);
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidOperationException("Could not determine current carousel title.");
            }

            return title!;
        }

        private string? TryGetCurrentCarouselTitle(IWebDriver webDriver)
        {
            By activeSlideLocator = By.XPath("//*[contains(@class,'slick-active') or contains(@class,'swiper-slide-active') or contains(@class,'is-active') or @aria-hidden='false']");
            By titleLocator = By.XPath(".//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//span[contains(@class,'museo-sans-light')]");

            var activeSlides = webDriver.FindElements(activeSlideLocator);
            IWebElement? activeSlide = activeSlides.FirstOrDefault(s => s.Displayed);
            if (activeSlide is null) return null;

            IWebElement? titleElement = activeSlide.FindElements(titleLocator).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
            if (titleElement is null) return null;

            string title = GetElementText(titleElement);
            return !string.IsNullOrWhiteSpace(title) ? title : null;
        }

        public IWebElement GetActiveCarouselSlide()
        {
            logger.LogInformation("Finding active carousel slide.");

            By activeSlideLocator = By.XPath("//*[contains(@class,'slick-active') or contains(@class,'swiper-slide-active') or contains(@class,'is-active') or @aria-hidden='false']");

            IWebElement? activeSlide = wait.Until(d =>
            {
                try
                {
                    return d.FindElements(activeSlideLocator).FirstOrDefault(s => s.Displayed);
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            if (activeSlide is null)
            {
                throw new InvalidOperationException("Could not locate the active carousel slide.");
            }

            ScrollToElement(activeSlide);

            return activeSlide;
        }

        public string GetCarouselArticleTitle(IWebElement activeSlide)
        {
            logger.LogInformation("Reading carousel article title.");

            By titleLocator = By.XPath(".//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//span[contains(@class,'museo-sans-light')]");

            IWebElement? titleElement = activeSlide.FindElements(titleLocator).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));

            if (titleElement is null)
            {
                throw new InvalidOperationException("Carousel title element was not found.");
            }

            string notedTitle = GetElementText(titleElement);

            if (string.IsNullOrWhiteSpace(notedTitle))
            {
                throw new InvalidOperationException("Carousel article title was empty.");
            }

            return notedTitle;
        }

        public void ClickCarouselArticleLink(IWebElement activeSlide)
        {
            logger.LogInformation("Finding carousel article link.");

            IWebElement? articleLink = activeSlide.FindElements(By.ClassName("slider-cta-link")).FirstOrDefault(e => e.Displayed && e.Enabled);

            if (articleLink is null)
            {
                By insightsLinkSelector = By.CssSelector("a[href*='/insights']:not([aria-hidden='true']), a[href*='/news']:not([aria-hidden='true']), a[href*='/thought-leadership']:not([aria-hidden='true'])");
                articleLink = activeSlide.FindElements(insightsLinkSelector).FirstOrDefault(e => e.Displayed && e.Enabled);
            }

            if (articleLink is null)
            {
                articleLink = activeSlide.FindElements(By.XPath(".//a | .//button")).FirstOrDefault(e => e.Displayed && e.Enabled);
            }

            if (articleLink is null)
            {
                throw new InvalidOperationException("Could not find article link inside active carousel slide.");
            }

            string href = articleLink!.GetAttribute("href") ?? string.Empty;

            logger.LogInformation("Clicking carousel article link. Text: '{Text}', href: '{Href}'", GetElementText(articleLink), href);

            ScrollToElement(articleLink);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", articleLink);

            logger.LogInformation("Carousel article link clicked.");
        }
    }
}
