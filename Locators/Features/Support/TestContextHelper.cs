using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO;
using NUnit.Framework;

namespace Locators.Features.Support
{
    public static class TestContextHelper
    {
        public static IWebDriver? Driver { get; set; }
        public static WebDriverWait? Wait { get; set; }
        public static ILogger? Logger { get; set; }
        public static ILoggerFactory? LoggerFactory { get; set; }
        public static string BaseUrl { get; private set; } = string.Empty;
        public static string DownloadDirectory { get; private set; } = string.Empty;

        public static void Init()
        {
            BaseUrl = ReadBaseUrl();

            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");

            // configure downloads directory (relative to test run working directory)
            DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Downloads");
            Directory.CreateDirectory(DownloadDirectory);

            options.AddUserProfilePreference("download.default_directory", DownloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

            // configure driver
            Driver = new ChromeDriver(options);
            Driver.Manage().Window.Maximize();
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

            // logger
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                });

                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            });

            Logger = LoggerFactory.CreateLogger("SpecFlow");

            // navigate
            Driver.Navigate().GoToUrl(BaseUrl);
        }

        public static void Cleanup()
        {
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
            finally
            {
                LoggerFactory?.Dispose();
            }
        }

        private static string ReadBaseUrl()
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

        public static void ScrollToAndClick(IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)Driver!).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
            }
            catch { }

            element.Click();
        }
    }
}
