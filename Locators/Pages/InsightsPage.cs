using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class InsightsPage(IWebDriver driver, WebDriverWait wait, ILogger logger) : BasePage(driver, wait, logger)
    {
        public bool WaitForInsightsContent()
        {
            logger.LogInformation("Waiting for Insights content.");

            bool contentLoaded = wait.Until(d => d.FindElements(By.XPath("//body//h1 | //body//h2 | //body//h3")).Any(e => e.Displayed));

            logger.LogInformation("Insights content loaded.");

            return contentLoaded;
        }

        public bool SwipeCarousel(int numberOfSwipes)
        {
            if (numberOfSwipes < 2)
            {
                logger.LogWarning("Carousel must be swiped at least twice.");
                return false;
            }

            logger.LogInformation("Swiping carousel {Count} times.", numberOfSwipes);

            By nextButtonLocator = By.XPath("//button[contains(@class,'next') or contains(@class,'slick-next') or contains(@class,'slider__right-arrow') or contains(@aria-label,'Next') or contains(@title,'Next')]");

            for (int i = 0; i < numberOfSwipes; i++)
            {
                string previousTitle = GetCurrentCarouselTitle();

                IWebElement? nextButton = driver.FindElements(nextButtonLocator).FirstOrDefault(b => b.Displayed && b.Enabled);

                if (nextButton is null)
                {
                    logger.LogWarning("Carousel Next button was not found.");
                    return false;
                }

                ScrollToElement(nextButton);

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", nextButton);

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
                    logger.LogWarning("Carousel did not change after swipe {Swipe}.", i + 1);
                    return false;
                }

                logger.LogInformation("Carousel swipe {Current}/{Total} completed.", i + 1, numberOfSwipes);
            }

            logger.LogInformation("Carousel swiping completed.");

            return true;
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

            return title ?? string.Empty;
        }

        private string? TryGetCurrentCarouselTitle(IWebDriver webDriver)
        {
            By activeSlideLocator = By.CssSelector(".slick-active, .swiper-slide-active, .is-active, :not([aria-hidden='true'])");
            By titleLocator = By.XPath(".//h1 | .//h2 | .//h3 | .//h4 | .//h5 | .//span[contains(@class,'museo-sans-light')]");

            var activeSlides = webDriver.FindElements(activeSlideLocator);
            IWebElement? activeSlide = activeSlides.FirstOrDefault(s => s.Displayed);
            if (activeSlide is null) return null;

            IWebElement? titleElement = activeSlide.FindElements(titleLocator).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
            if (titleElement is null) return null;

            string title = GetElementText(titleElement);
            return !string.IsNullOrWhiteSpace(title) ? title : null;
        }

        public IWebElement? GetActiveCarouselSlide()
        {
            logger.LogInformation("Finding active carousel slide.");

            By activeSlideLocator = By.CssSelector(".slick-active, .swiper-slide-active, .is-active, :not([aria-hidden='true'])");

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
                logger.LogWarning("Could not locate the active carousel slide.");
                return null;
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
                logger.LogWarning("Carousel title element was not found.");
                return string.Empty;
            }

            string notedTitle = GetElementText(titleElement);

            if (string.IsNullOrWhiteSpace(notedTitle))
            {
                logger.LogWarning("Carousel article title was empty.");
                return string.Empty;
            }

            return notedTitle;
        }

        public void ClickCarouselArticleLink(IWebElement activeSlide)
        {
            logger.LogInformation("Finding carousel article link.");

            IWebElement? articleLink = activeSlide.FindElements(By.ClassName("slider-cta-link")).FirstOrDefault(e => e.Displayed && e.Enabled);

            if (articleLink is null)
            {
                articleLink = activeSlide.FindElements(By.XPath(".//a[contains(@href,'/insights') or contains(@href,'/news') or contains(@href,'/thought-leadership')]")).FirstOrDefault(e => e.Displayed && e.Enabled);
            }

            if (articleLink is null)
            {
                articleLink = activeSlide.FindElements(By.XPath(".//a | .//button")).FirstOrDefault(e => e.Displayed && e.Enabled);
            }

            if (articleLink is null)
            {
                logger.LogWarning("Could not find article link inside active carousel slide.");
                return;
            }

            string href = articleLink.GetAttribute("href") ?? string.Empty;

            logger.LogInformation("Clicking carousel article link. Text: '{Text}', href: '{Href}'", GetElementText(articleLink), href);

            ScrollToElement(articleLink);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", articleLink);

            logger.LogInformation("Carousel article link clicked.");

            return;
        }
    }
}
