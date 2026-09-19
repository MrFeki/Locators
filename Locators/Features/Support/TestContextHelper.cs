using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.IO;
using NUnit.Framework;

namespace Locators.Features.Support
{
    public static class TestContextHelper
    {
        private class Context
        {
            public IWebDriver? Driver;
            public WebDriverWait? Wait;
            public ILogger? Logger;
            public ILoggerFactory? LoggerFactory;
            public string BaseUrl = string.Empty;
            public string DownloadDirectory = string.Empty;
        }

        private static readonly AsyncLocal<Context?> _current = new AsyncLocal<Context?>();

        private static Context Current
        {
            get => _current.Value ?? throw new InvalidOperationException("Test context is not initialized for the current scenario.");
            set => _current.Value = value;
        }

        public static IWebDriver? Driver => _current.Value?.Driver;
        public static WebDriverWait? Wait => _current.Value?.Wait;
        public static ILogger? Logger => _current.Value?.Logger;
        public static ILoggerFactory? LoggerFactory => _current.Value?.LoggerFactory;
        public static string BaseUrl => _current.Value?.BaseUrl ?? string.Empty;
        public static string DownloadDirectory => _current.Value?.DownloadDirectory ?? string.Empty;

        public static void Init()
        {
            var ctx = new Context();
            ctx.BaseUrl = ReadBaseUrl();
            ctx.DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Downloads");
            Directory.CreateDirectory(ctx.DownloadDirectory);

            var browser = (Environment.GetEnvironmentVariable("BROWSER") ?? "chrome").ToLowerInvariant();
            var headless = string.Equals(Environment.GetEnvironmentVariable("HEADLESS"), "true", StringComparison.OrdinalIgnoreCase);

            if (browser == "edge")
            {
                var options = new EdgeOptions();
                options.AddArgument("--disable-notifications");
                if (headless) options.AddArgument("--headless=new");
                options.AddUserProfilePreference("download.default_directory", ctx.DownloadDirectory);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("download.directory_upgrade", true);
                options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                ctx.Driver = new EdgeDriver(options);
            }
            else
            {
                var options = new ChromeOptions();
                options.AddArgument("--disable-notifications");
                if (headless) options.AddArgument("--headless=new");
                options.AddUserProfilePreference("download.default_directory", ctx.DownloadDirectory);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("download.directory_upgrade", true);
                options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
                ctx.Driver = new ChromeDriver(options);
            }

            ctx.Driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
            ctx.Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            ctx.Wait = new WebDriverWait(ctx.Driver, TimeSpan.FromSeconds(10));

            ctx.LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                });
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            });

            ctx.Logger = ctx.LoggerFactory.CreateLogger("SpecFlow");
            Current = ctx;
            ctx.Driver.Navigate().GoToUrl(ctx.BaseUrl);
        }

        public static string CaptureScreenshot(string scenarioName)
        {
            if (Driver is not ITakesScreenshot screenshotDriver) return string.Empty;
            var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
            Directory.CreateDirectory(directory);
            foreach (var c in Path.GetInvalidFileNameChars()) scenarioName = scenarioName.Replace(c, '_');
            var path = Path.Combine(directory, $"{scenarioName}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png");
            screenshotDriver.GetScreenshot().SaveAsFile(path);
            return path;
        }

        public static void Cleanup()
        {
            var ctx = _current.Value;
            if (ctx is null) return;
            try
            {
                ctx.Driver?.Quit();
                ctx.Driver?.Dispose();
                ctx.LoggerFactory?.Dispose();
            }
            finally
            {
                _current.Value = null;
            }
        }

        private static string ReadBaseUrl()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
            var value = config["BaseUrl"];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"BaseUrl was not found in appsettings.json located in '{AppContext.BaseDirectory}'.");
            return value;
        }

        public static void ScrollToAndClick(IWebElement element)
        {
            try { ((IJavaScriptExecutor)Driver!).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element); } catch { }
            element.Click();
        }
    }
}
