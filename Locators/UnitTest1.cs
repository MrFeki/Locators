using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
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

            // instantiate page objects
            home = new HomePage(driver, wait, logger);
            careers = new CareersPage(driver, wait, logger);
            globalSearch = new GlobalSearchPage(driver, wait, logger);
            insights = new InsightsPage(driver, wait, logger);
            article = new ArticlePage(driver, wait, logger);

            // provide download directory to home page
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
            careers.WaitForJobResults();
            careers.ValidateLatestJobContainsLanguage(programmingLanguage);
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
            globalSearch.ValidateAllLinksContainWord(query);
        }

        [Test]
        [TestCase("Code-Of-Conduct_01_26.pdf")]
        public void ValidateFileDownload(string expectedFileName)
        {
            logger.LogInformation("Starting file download validation for {FileName}", expectedFileName);

            string expectedFilePath = Path.Combine(downloadDirectory, expectedFileName);

            if (File.Exists(expectedFilePath)) File.Delete(expectedFilePath);

            home.ScrollToFooter();
            var link = home.FindCodeOfConductLink(expectedFileName);
            home.ClickDownloadLink(link);

            bool downloaded = home.WaitForFileDownload(expectedFilePath);

            Assert.That(downloaded, Is.True, $"Expected file '{expectedFileName}' was not downloaded.");
            Assert.That(File.Exists(expectedFilePath), Is.True, $"Expected file '{expectedFileName}' does not exist in the download directory.");
            long fileSize = new FileInfo(expectedFilePath).Length;
            Assert.That(fileSize, Is.GreaterThan(0), $"Downloaded file '{expectedFileName}' is empty.");

            logger.LogInformation("SUCCESS: File {FileName} downloaded. Path: {FilePath}, Size: {FileSize} bytes", expectedFileName, expectedFilePath, fileSize);
        }

        [Test]
        public void ValidateCarouselArticleTitle()
        {
            logger.LogInformation("Starting carousel article title validation.");

            home.ClickInsights();
            insights.WaitForInsightsContent();
            insights.SwipeCarousel(3);
            var active = insights.GetActiveCarouselSlide();
            string notedTitle = insights.GetCarouselArticleTitle(active);

            Assert.That(notedTitle, Is.Not.Empty, "Could not determine the visible carousel article title.");

            logger.LogInformation("Noted carousel article title: {Title}", notedTitle);

            insights.ClickCarouselArticleLink(active);
            string pageTitle = article.GetArticlePageTitle();

            Assert.That(pageTitle, Is.Not.Empty, "Could not determine article page title.");
            logger.LogInformation("Article page title: {Title}", pageTitle);

            Assert.That(pageTitle, Does.StartWith(notedTitle).IgnoreCase, $"Article page title '{pageTitle}' does not match carousel title '{notedTitle}'.");

            logger.LogInformation("SUCCESS: Carousel article title matches article page title: {Title}", notedTitle);
        }
    }
}