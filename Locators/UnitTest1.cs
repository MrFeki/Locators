using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using Locators.Pages;

namespace Locators
{
    public class Tests
    {
        private ChromeDriver driver = null!;
        private WebDriverWait wait = null!;

        private ILoggerFactory loggerFactory = null!;
        private ILogger<Tests> logger = null!;

        private string downloadDirectory = string.Empty;

        private HomePage home = null!;
        private CareersPage careers = null!;
        private GlobalSearchPage globalSearch = null!;
        private InsightsPage insights = null!;
        private ArticlePage article = null!;

        private static readonly string[] SearchKeywords =
        {
            "BLOCKCHAIN",
            "Cloud",
            "Automation"
        };

        [SetUp]
        public void Setup()
        {
            downloadDirectory =
                Path.Combine(
                    TestContext.CurrentContext.WorkDirectory,
                    "Downloads");

            Directory.CreateDirectory(downloadDirectory);

            var options = new ChromeOptions();

            options.AddUserProfilePreference("download.default_directory", downloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            options.AddArgument("--disable-notifications");

            driver = new ChromeDriver(options);

            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

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

            home = new HomePage(driver, wait, logger);
            careers = new CareersPage(driver, wait, logger);
            globalSearch = new GlobalSearchPage(driver, wait, logger);
            insights = new InsightsPage(driver, wait, logger);
            article = new ArticlePage(driver, wait, logger);

            home.DownloadDirectory = downloadDirectory;

            logger.LogInformation("Setup complete. Navigated to {BaseUrl}", BaseUrl);
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
            driver?.Dispose();

            logger?.LogInformation("Teardown complete.");

            loggerFactory?.Dispose();
        }

        private static string BaseUrl
        {
            get
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

                var config = builder.Build();

                string? value = config["BaseUrl"];

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"BaseUrl was not found in appsettings.json located in '{AppContext.BaseDirectory}'.");
                }

                return value;
            }
        }

        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return Regex.Replace(input, "\\s+", " ").Trim();
        }

        [Test]
        [TestCase("Python", "Mexico")]
        [TestCase("Java", "Serbia")]
        [TestCase(".NET", "Ukraine")]
        public void ValidateSearchPosition(string programmingLanguage, string country)
        {
            logger.LogInformation("Starting career search for {Language} in {Country}", programmingLanguage, country);

            home.ClickCareers();
            careers.ClickStartYourSearch();
            careers.SearchForProgrammingLanguage(programmingLanguage);
            careers.ChooseCountry(country);
            careers.SetCheckboxRemote();
            careers.ClickSubmitButton();
            bool jobsLoaded = careers.WaitForJobResults();

            var jobText = careers.ValidateFirstJobContainsLanguage(programmingLanguage);

            Assert.Multiple(() =>
            {
                Assert.That(jobsLoaded, Is.True, "Job results did not load.");
                Assert.That(jobText, Is.Not.Null.And.Not.Empty, $"Latest job does not contain '{programmingLanguage}'.");
                Assert.That(jobText, Does.Contain(programmingLanguage).IgnoreCase, $"Latest job does not contain '{programmingLanguage}'.");
            });
        }

        [Test]
        [TestCaseSource(nameof(SearchKeywords))]
        public void ValidateGlobalSearch(string query)
        {
            logger.LogInformation("Starting global search for {Query}", query);

            home.ClickSearchIcon();
            var input = home.GetGlobalSearchInput();
            globalSearch.EnterGlobalSearchQuery(input, query);
            globalSearch.ClickGlobalFindButton();
            globalSearch.WaitForGlobalSearchResults();
            Assert.That(globalSearch.ValidateAllLinksContainWord(query), Is.True, $"No search result matched '{query}'.");
        }

        [Test]
        [TestCase("Code_of_Ethical_Conduct.pdf")]
        public void ValidateFileDownload(string expectedFileName)
        {
            logger.LogInformation("Starting file download validation for {FileName}", expectedFileName);

            home.ScrollToFooter();
            var (link, resolvedFileName) = home.FindCodeOfConductLink(expectedFileName);


            if (!string.IsNullOrWhiteSpace(resolvedFileName) &&
                !string.Equals(resolvedFileName, expectedFileName, StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail($"Resolved link filename '{resolvedFileName}' does not match expected filename '{expectedFileName}'.");
            }

            string actualFileName = expectedFileName;
            string actualFilePath = Path.Combine(downloadDirectory, actualFileName);

            if (File.Exists(actualFilePath)) File.Delete(actualFilePath);

            logger.LogInformation("Attempting to download file '{ExpectedName}', resolved link filename '{ResolvedName}'", expectedFileName, actualFileName);

            home.ClickDownloadLink(link);

            bool downloaded = home.WaitForFileDownload(actualFilePath);

            Assert.Multiple(() =>
            {
                Assert.That(downloaded, Is.True, $"Expected file '{actualFileName}' was not downloaded to '{actualFilePath}'.");
                Assert.That(File.Exists(actualFilePath), Is.True, $"Expected file '{actualFileName}' does not exist at '{actualFilePath}'.");

                // Add fileSize calculation and assertion
                var fileSize = File.Exists(actualFilePath) ? new FileInfo(actualFilePath).Length : 0;
                Assert.That(fileSize, Is.GreaterThan(0), $"Downloaded file '{actualFileName}' is empty.");
            });
        }

        [Test]
        public void ValidateCarouselArticleTitle()
        {
            logger.LogInformation("Starting carousel article title validation.");

            home.ClickInsights();
            bool insightsLoaded = insights.WaitForInsightsContent();
            Assert.That(insightsLoaded, Is.True, "Insights content did not load.");

            bool swipeOk = insights.SwipeCarousel(3);
            Assert.That(swipeOk, Is.True, "Carousel did not swipe correctly.");

            var active = insights.GetActiveCarouselSlide();
            Assert.That(active, Is.Not.Null, "Could not locate the active carousel slide.");

            string notedTitle = insights.GetCarouselArticleTitle(active!);
            Assert.That(notedTitle, Is.Not.Empty, "Could not determine the visible carousel article title.");

            logger.LogInformation("Noted carousel article title: {Title}", notedTitle);

            insights.ClickCarouselArticleLink(active!);

            string pageTitle = article.GetArticlePageTitle();
            Assert.That(pageTitle, Is.Not.Empty, "Could not determine article page title.");
            logger.LogInformation("Article page title: {Title}", pageTitle);

            Assert.That(pageTitle, Does.StartWith(notedTitle).IgnoreCase, $"Article page title '{pageTitle}' does not match carousel title '{notedTitle}'.");

            logger.LogInformation("SUCCESS: Carousel article title matches article page title: {Title}", notedTitle);
        }
    }
}
