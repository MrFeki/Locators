using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Locators
{
    public class Tests
    {
        private ChromeDriver? driver;
        private WebDriverWait? wait;

        private ILoggerFactory? loggerFactory;
        private ILogger<Tests>? logger;

        private static readonly string[] SearchKeywords =
        {
            "BLOCKCHAIN",
            "Cloud",
            "Automation"
        };

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();

            options.AddArgument("--disable-notifications");

            driver = new ChromeDriver(options);

            driver.Manage().Window.Maximize();

            driver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromSeconds(3);

            wait = new WebDriverWait(
                driver,
                TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl(BaseUrl);

            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                });

                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            });

            logger = loggerFactory.CreateLogger<Tests>();

            logger.LogInformation(
                "Setup complete. Navigated to {BaseUrl}",
                BaseUrl);
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
            driver?.Dispose();

            logger?.LogInformation(
                "Teardown complete.");

            loggerFactory?.Dispose();

            driver = null;
            wait = null;
            logger = null;
            loggerFactory = null;
        }

        private static string BaseUrl
        {
            get
            {
                var builder =
                    new ConfigurationBuilder()
                        .SetBasePath(
                            AppContext.BaseDirectory)
                        .AddJsonFile(
                            "appsettings.json",
                            optional: false,
                            reloadOnChange: false);

                var config = builder.Build();

                string? value =
                    config["BaseUrl"];

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException(
                        $"BaseUrl was not found in " +
                        $"appsettings.json located in " +
                        $"'{AppContext.BaseDirectory}'.");
                }

                return value;
            }
        }

        [Test]
        [TestCase("Java", "Serbia")]
        public void ValidateSearchPosition(
            string programmingLanguage,
            string country)
        {
            logger?.LogInformation(
                "Starting career search for " +
                "{Language} in {Country}",
                programmingLanguage,
                country);

            FindCareersLinkAndClick();

            ClickStartYourSearchButton();

            SearchForProgrammingLanguage(
                programmingLanguage);

            ChooseCountry(country);

            SetCheckboxRemote();

            ClickSubmitButton();

            WaitForJobResults();

            ValidateLatestJobContainsLanguage(
                programmingLanguage);
        }

        [Test]
        [TestCaseSource(nameof(SearchKeywords))]
        public void ValidateGlobalSearch(
            string query)
        {
            logger?.LogInformation(
                "Starting global search for {Query}",
                query);

            ClickSearchIcon();

            IWebElement searchInput =
                GetGlobalSearchInput();

            EnterGlobalSearchQuery(
                searchInput,
                query);

            ClickGlobalFindButton();

            WaitForGlobalSearchResults();

            ValidateAllLinksContainWord(query);
        }

        private IWebElement WaitForVisibleAndEnabled(
            By locator)
        {
            if (wait is null)
            {
                throw new InvalidOperationException(
                    "WebDriverWait is not initialized.");
            }

            return wait.Until(d =>
            {
                var elements =
                    d.FindElements(locator);

                return elements
                    .FirstOrDefault(element =>
                        element.Displayed &&
                        element.Enabled);

            }) ?? throw new InvalidOperationException(
                $"Element was not found: {locator}");
        }

        private void ScrollToElement(
            IWebElement element)
        {
            if (driver is null)
            {
                throw new InvalidOperationException(
                    "WebDriver is not initialized.");
            }

            ((IJavaScriptExecutor)driver)
                .ExecuteScript(
                    "arguments[0].scrollIntoView(" +
                    "{block:'center'});",
                    element);
        }

        private void MoveToElement(
            IWebElement element)
        {
            if (driver is null)
            {
                throw new InvalidOperationException(
                    "WebDriver is not initialized.");
            }

            new Actions(driver)
                .MoveToElement(element)
                .Perform();
        }

        private void FindCareersLinkAndClick()
        {
            logger?.LogInformation(
                "Finding Careers link.");

            IWebElement careersLink =
                WaitForVisibleAndEnabled(
                    By.LinkText("Careers"));

            careersLink.Click();

            logger?.LogInformation(
                "Careers link clicked.");
        }

        private void ClickStartYourSearchButton()
        {
            logger?.LogInformation(
                "Finding Start Your Search button.");

            IWebElement startSearchButton =
                WaitForVisibleAndEnabled(
                    By.ClassName("button-body"));

            ScrollToElement(
                startSearchButton);

            MoveToElement(
                startSearchButton);

            startSearchButton.Click();

            logger?.LogInformation(
                "Start Your Search button clicked.");
        }

        private void SearchForProgrammingLanguage(
            string programmingLanguage)
        {
            logger?.LogInformation(
                "Entering programming language: {Language}",
                programmingLanguage);

            IWebElement searchInput =
                WaitForVisibleAndEnabled(
                    By.CssSelector(
                        "input[data-testid='search-input']"));

            ScrollToElement(
                searchInput);

            searchInput.Clear();

            searchInput.SendKeys(
                programmingLanguage);

            logger?.LogInformation(
                "Programming language entered.");
        }

        private void ChooseCountry(
            string country)
        {
            logger?.LogInformation(
                "Selecting country: {Country}",
                country);

            IWebElement countryInput =
                WaitForVisibleAndEnabled(
                    By.XPath(
                        "//div[@data-testid='country-dropdown']" +
                        "//input[@role='combobox']"));

            ScrollToElement(
                countryInput);

            MoveToElement(
                countryInput);

            countryInput.Click();

            countryInput.Clear();

            countryInput.SendKeys(
                country);

            countryInput.SendKeys(
                Keys.Enter);

            logger?.LogInformation(
                "Country {Country} selected.",
                country);
        }

        private void SetCheckboxRemote()
        {
            logger?.LogInformation(
                "Selecting Remote checkbox.");

            IWebElement remoteLabel =
                WaitForVisibleAndEnabled(
                    By.XPath(
                        "//label[" +
                        ".//span[normalize-space()='Remote']" +
                        "]"));

            ScrollToElement(
                remoteLabel);

            MoveToElement(
                remoteLabel);

            try
            {
                remoteLabel.Click();
            }
            catch (ElementClickInterceptedException)
            {
                HandleCookieBanner();

                ScrollToElement(
                    remoteLabel);

                MoveToElement(
                    remoteLabel);

                remoteLabel.Click();
            }

            logger?.LogInformation(
                "Remote checkbox selected.");
        }

        private void HandleCookieBanner()
        {
            if (driver is null)
            {
                throw new InvalidOperationException(
                    "WebDriver is not initialized.");
            }

            try
            {
                var buttons =
                    driver.FindElements(
                        By.Id(
                            "onetrust-accept-btn-handler"));

                IWebElement? acceptButton =
                    buttons.FirstOrDefault(button =>
                        button.Displayed &&
                        button.Enabled);

                if (acceptButton is not null)
                {
                    acceptButton.Click();

                    logger?.LogInformation(
                        "Cookie banner accepted.");
                }
            }
            catch
            {
                logger?.LogInformation(
                    "Cookie banner not present.");
            }
        }

        private void ClickSubmitButton()
        {
            logger?.LogInformation(
                "Finding Search button.");

            IWebElement submitButton =
                WaitForVisibleAndEnabled(
                    By.Name(
                        "submit_search_box_button"));

            ScrollToElement(
                submitButton);

            MoveToElement(
                submitButton);

            submitButton.Click();

            logger?.LogInformation(
                "Search button clicked.");
        }

        private void WaitForJobResults()
        {
            if (wait is null)
            {
                throw new InvalidOperationException(
                    "WebDriverWait is not initialized.");
            }

            logger?.LogInformation(
                "Waiting for job results.");

            wait.Until(d =>
                d.FindElements(
                    By.CssSelector(
                        "[data-testid=" +
                        "'accordion-section-container']"))
                    .Count > 0);

            logger?.LogInformation(
                "Job results loaded.");
        }

        private void ValidateLatestJobContainsLanguage(
            string programmingLanguage)
        {
            if (driver is null ||
                wait is null)
            {
                throw new InvalidOperationException(
                    "WebDriver is not initialized.");
            }

            logger?.LogInformation(
                "Validating latest result.");

            IReadOnlyCollection<IWebElement>
                jobResults =
                    driver.FindElements(
                        By.CssSelector(
                            "[data-testid=" +
                            "'accordion-section-container']"));

            Assert.That(
                jobResults,
                Is.Not.Empty,
                "No job results were found.");

            IWebElement latestJob =
                jobResults.First();

            IWebElement expandButton =
                latestJob.FindElement(
                    By.CssSelector(
                        "[data-testid=" +
                        "'accordion-section-header-icon-container']"));

            ScrollToElement(
                expandButton);

            wait.Until(_ =>
            {
                try
                {
                    return
                        expandButton.Displayed &&
                        expandButton.Enabled;
                }
                catch (
                    StaleElementReferenceException)
                {
                    return false;
                }
            });

            try
            {
                expandButton.Click();
            }
            catch (
                ElementClickInterceptedException)
            {
                ScrollToElement(
                    expandButton);

                MoveToElement(
                    expandButton);

                expandButton.Click();
            }

            IWebElement expandedDetails =
                wait.Until(_ =>
                {
                    try
                    {
                        IWebElement details =
                            latestJob.FindElement(
                                By.CssSelector(
                                    "[data-testid=" +
                                    "'accordion-section-children-container']"));

                        return details.Displayed
                            ? details
                            : null;
                    }
                    catch (
                        StaleElementReferenceException)
                    {
                        return null;
                    }
                })
                ?? throw new InvalidOperationException(
                    "Expanded job details " +
                    "were not found.");

            Assert.That(
                expandedDetails.Text,
                Does.Contain(
                    programmingLanguage)
                    .IgnoreCase,
                $"Latest job does not contain " +
                $"'{programmingLanguage}'.");

            logger?.LogInformation(
                "Latest job contains {Language}.",
                programmingLanguage);
        }

        private void ClickSearchIcon()
        {
            IWebElement searchIcon =
                WaitForVisibleAndEnabled(
                    By.XPath(
                        "//span[" +
                        "contains(@class," +
                        "'header-search__search-icon')" +
                        "]"));

            searchIcon.Click();

            logger?.LogInformation(
                "Global search icon clicked.");
        }

        private IWebElement GetGlobalSearchInput()
        {
            return WaitForVisibleAndEnabled(
                By.Id("new_form_search"));
        }

        private void EnterGlobalSearchQuery(
            IWebElement searchInput,
            string query)
        {
            searchInput.Clear();

            searchInput.SendKeys(
                query);

            logger?.LogInformation(
                "Search query entered: {Query}",
                query);
        }

        private void ClickGlobalFindButton()
        {
            IWebElement findButton =
                WaitForVisibleAndEnabled(
                    By.XPath(
                        "//span[normalize-space()='Find']" +
                        "/ancestor::button[" +
                        "contains(@class," +
                        "'custom-search-button')" +
                        "]"));

            findButton.Click();

            logger?.LogInformation(
                "Find button clicked.");
        }

        private void WaitForGlobalSearchResults()
        {
            if (wait is null)
            {
                throw new InvalidOperationException(
                    "WebDriverWait is not initialized.");
            }

            wait.Until(d =>
                d.FindElements(
                    By.CssSelector(
                        ".search-results__title-link"))
                    .Count > 0);

            logger?.LogInformation(
                "Global search results loaded.");
        }

        private void ValidateAllLinksContainWord(
            string expectedWord)
        {
            if (driver is null)
            {
                throw new InvalidOperationException(
                    "WebDriver is not initialized.");
            }

            var terms =
                (expectedWord ?? string.Empty)
                    .Split(
                        new[] { '/' },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(t =>
                        t.Trim().Trim(
                            '"',
                            '“',
                            '”',
                            '\'',
                            '‘',
                            '’'))
                    .Where(t =>
                        !string.IsNullOrEmpty(t))
                    .ToList();

            if (!terms.Any())
            {
                Assert.Fail(
                    "No expected terms were provided for validation.");
            }

            var links =
                driver.FindElements(
                    By.CssSelector(
                        ".search-results__title-link"));

            Assert.That(
                links,
                Is.Not.Empty,
                "No result links were found.");

            var termMatches =
                terms.ToDictionary(
                    term => term,
                    term => new List<string>());

            var invalidLinks =
                new List<string>();

            foreach (var link in links)
            {
                string text;

                try
                {
                    text =
                        GetElementText(link);
                }
                catch
                {
                    text =
                        string.Empty;
                }

                string href =
                    string.Empty;

                try
                {
                    href =
                        link.GetAttribute("href")
                        ?? string.Empty;
                }
                catch
                {
                }

                var matchedTerms =
                    terms
                        .Where(term =>
                            (!string.IsNullOrEmpty(text) &&
                             text.IndexOf(
                                 term,
                                 StringComparison.OrdinalIgnoreCase) >= 0)
                            ||
                            (!string.IsNullOrEmpty(href) &&
                             href.IndexOf(
                                 term,
                                 StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();

                if (matchedTerms.Any())
                {
                    foreach (var matchedTerm in matchedTerms)
                    {
                        termMatches[matchedTerm]
                            .Add(text);
                    }

                    logger?.LogInformation(
                        "MATCH -> Text: '{Text}', href: '{Href}', Terms: {Terms}",
                        text,
                        href,
                        string.Join(
                            ",",
                            matchedTerms));
                }
                else
                {
                    invalidLinks.Add(text);

                    logger?.LogWarning(
                        "NO MATCH -> Text: '{Text}', href: '{Href}'",
                        text,
                        href);
                }
            }

            var missingTerms =
                termMatches
                    .Where(result =>
                        !result.Value.Any())
                    .Select(result =>
                        result.Key)
                    .ToList();

            if (missingTerms.Any())
            {
                Assert.Fail(
                    $"No links found containing the expected term(s): " +
                    $"{string.Join(", ", missingTerms)}.\n" +
                    $"All terms attempted: " +
                    $"{string.Join(", ", terms)}.\n" +
                    $"Links observed: " +
                    $"{string.Join(" | ", links.Select(GetElementText))}");
            }

            if (invalidLinks.Any())
            {
                logger?.LogInformation(
                    "{Count} link(s) did not match expected term(s): {Examples}",
                    invalidLinks.Count,
                    string.Join(
                        ", ",
                        invalidLinks.Take(5)));
            }
        }

        private static string GetElementText(
            IWebElement element)
        {
            string text =
                element.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                text =
                    element.GetAttribute(
                        "textContent")
                    ??
                    element.GetAttribute(
                        "innerText")
                    ??
                    string.Empty;
            }

            return System.Text.RegularExpressions.Regex
                .Replace(
                    text,
                    "\\s+",
                    " ")
                .Trim();
        }
    }
}