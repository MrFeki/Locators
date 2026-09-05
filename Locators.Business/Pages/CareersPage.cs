using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class CareersPage : BasePage
    {
        public CareersPage(IWebDriver driver, WebDriverWait wait, ILogger logger)
            : base(driver, wait, logger)
        {
        }

        public void ClickStartYourSearch()
        {
            logger.LogInformation("Finding Start Your Search button.");

            var startSearchButton = WaitForVisibleAndEnabled(By.ClassName("button-body"));

            ScrollToElement(startSearchButton);
            MoveToElement(startSearchButton);
            startSearchButton.Click();

            logger.LogInformation("Start Your Search button clicked.");
        }

        public void SearchForProgrammingLanguage(string programmingLanguage)
        {
            logger.LogInformation("Entering programming language: {Language}", programmingLanguage);

            var searchInput = WaitForVisibleAndEnabled(By.CssSelector("input[data-testid='search-input']"));
            ScrollToElement(searchInput);
            searchInput.Clear();
            searchInput.SendKeys(programmingLanguage);

            logger.LogInformation("Programming language entered.");
        }

        public void ChooseCountry(string country)
        {
            logger.LogInformation("Selecting country: {Country}", country);

            var countryInput = WaitForVisibleAndEnabled(By.XPath("//div[@data-testid='country-dropdown']//input[@role='combobox']"));
            ScrollToElement(countryInput);
            MoveToElement(countryInput);
            countryInput.Click();
            countryInput.Clear();
            countryInput.SendKeys(country + Keys.Enter);

            logger.LogInformation("Country {Country} selected.", country);
        }

        public void SetCheckboxRemote()
        {
            logger.LogInformation("Selecting Remote checkbox.");

            By remoteLocator = By.XPath("//label[.//span[normalize-space()='Remote']]");
            var remoteLabel = WaitForVisibleAndEnabled(remoteLocator);
            ScrollToElement(remoteLabel);
            MoveToElement(remoteLabel);

            try
            {
                remoteLabel.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // try to accept cookie banner via framework helper and retry click
                _ = Locators.Core.WebDriver.BrowserHelpers.TryAcceptCookieBanner(driver);

                remoteLabel = WaitForVisibleAndEnabled(remoteLocator);
                ScrollToElement(remoteLabel);
                MoveToElement(remoteLabel);
                Locators.Core.WebDriver.BrowserHelpers.SafeClick(driver, remoteLabel);
            }

            logger.LogInformation("Remote checkbox selected.");
        }

        // Cookie banner handling moved to BrowserHelpers in Core to centralize behaviour.

        public void ClickSubmitButton()
        {
            logger.LogInformation("Finding Search button.");

            By jobSelector = By.CssSelector("[data-testid='accordion-section-container']");

            IWebElement? oldFirstJob = driver.FindElements(jobSelector).FirstOrDefault();

            const int maxAttempts = 5;
            var clicked = false;

            for (int attempt = 1; attempt <= maxAttempts && !clicked; attempt++)
            {
                try
                {
                    var submitButton = WaitForVisibleAndEnabled(By.Name("submit_search_box_button"));

                    ScrollToElement(submitButton);
                    MoveToElement(submitButton);
                    submitButton.Click();

                    clicked = true;
                    logger.LogInformation("Search button clicked (attempt {Attempt}).", attempt);
                }
                catch (StaleElementReferenceException)
                {
                    logger.LogWarning("Search button was stale on attempt {Attempt}, retrying.", attempt);
                    Thread.Sleep(250);
                }
                catch (ElementClickInterceptedException)
                {
                    logger.LogWarning("Click intercepted on attempt {Attempt}, attempting to handle cookie banner and retry.", attempt);
                    _ = Locators.Core.WebDriver.BrowserHelpers.TryAcceptCookieBanner(driver);
                    Thread.Sleep(250);
                }
            }

            if (!clicked)
            {

                throw new StaleElementReferenceException("Failed to click Search button because it kept becoming stale.");
            }

            if (oldFirstJob is not null)
            {
                wait.Until(d =>
                {
                    try
                    {
                        oldFirstJob.GetAttribute("data-testid");
                        return false;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return true;
                    }
                });

                logger.LogInformation("Previous job results were replaced.");
            }
        }

        public void WaitForJobResults()
        {
            logger.LogInformation("Waiting for new job results.");

            By jobSelector = By.CssSelector("[data-testid='accordion-section-container']");

            bool resultsLoaded = wait.Until(d =>
            {
                try
                {
                    var jobs = d.FindElements(jobSelector);
                    if (jobs.Count == 0) return false;
                    var firstJob = jobs.First();
                    return firstJob.Displayed && !string.IsNullOrWhiteSpace(firstJob.Text);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });

            if (!resultsLoaded)
            {
                throw new InvalidOperationException("Job results did not load.");
            }

            logger.LogInformation("New job results loaded.");
        }
        public string GetLatestJobDetails()
        {
            logger.LogInformation("Retrieving latest job details.");

            By jobSelector = By.CssSelector("[data-testid='accordion-section-container']");
            By expandButtonSelector = By.CssSelector("[data-testid='accordion-section-header-icon-container']");
            By detailsSelector = By.CssSelector("[data-testid='accordion-section-children-container']");

            var jobResults = driver.FindElements(jobSelector);
            if (jobResults.Count == 0)
            {
                throw new InvalidOperationException("No job results were found.");
            }

            IWebElement? expandButton = wait.Until(d =>
            {
                try
                {
                    var jobs = d.FindElements(jobSelector);
                    if (jobs.Count == 0) return null;
                    var button = jobs.First().FindElements(expandButtonSelector).FirstOrDefault();
                    if (button is null) return null;
                    return button.Displayed && button.Enabled ? button : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            if (expandButton is null)
            {
                throw new InvalidOperationException("Expand button was not found.");
            }

            ScrollToElement(expandButton);
            MoveToElement(expandButton);

            IWebElement? freshExpandButton = wait.Until(d =>
            {
                try
                {
                    var jobs = d.FindElements(jobSelector);
                    if (jobs.Count == 0) return null;
                    var button = jobs.First().FindElements(expandButtonSelector).FirstOrDefault();
                    if (button is null) return null;
                    return button.Displayed && button.Enabled ? button : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            if (freshExpandButton is null)
            {
                throw new InvalidOperationException("Expand button was not ready for clicking.");
            }

            freshExpandButton.Click();

            logger.LogInformation("Latest job expanded.");

            var detailsWait = Locators.Core.WebDriver.WaitFactory.Create(driver, 15);

            string? jobText = detailsWait.Until(d =>
            {
                try
                {
                    var jobs = d.FindElements(jobSelector);
                    if (jobs.Count == 0) return null;
                    var details = jobs.First().FindElements(detailsSelector).FirstOrDefault();
                    if (details is null || !details.Displayed) return null;
                    string text = details.Text;
                    if (string.IsNullOrWhiteSpace(text)) return null;
                    logger.LogInformation("Retrieved expanded job details. Current text length: {Length}", text.Length);
                    return text;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            if (string.IsNullOrWhiteSpace(jobText))
            {
                throw new InvalidOperationException("Latest job details could not be obtained.");
            }

            return jobText;
        }

      
    }
}
