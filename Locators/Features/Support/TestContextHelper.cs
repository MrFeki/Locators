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
        // Per-scenario context stored in AsyncLocal so parallel scenarios do not interfere.
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
            get => _current.Value ?? throw new InvalidOperationException("Test context is not initialized for the current scenario. Ensure SpecFlow hooks created the context.");
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
            // create and assign a new per-scenario context
            var ctx = new Context();
            ctx.BaseUrl = ReadBaseUrl();

            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");

            // configure downloads directory (relative to test run working directory)
            ctx.DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Downloads");
            Directory.CreateDirectory(ctx.DownloadDirectory);

            options.AddUserProfilePreference("download.default_directory", ctx.DownloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

            // configure driver
            ctx.Driver = new ChromeDriver(options);
            ctx.Driver.Manage().Window.Maximize();
            ctx.Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            ctx.Wait = new WebDriverWait(ctx.Driver, TimeSpan.FromSeconds(10));

            // logger
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

            // assign to AsyncLocal current
            Current = ctx;

            // navigate
            try
            {
                ctx.Driver.Navigate().GoToUrl(ctx.BaseUrl);
            }
            catch
            {
                // swallow; test steps will report navigation failures explicitly
            }
        }

        public static void Cleanup()
        {
            var ctx = _current.Value;
            if (ctx is null) return;

            try
            {
                try
                {
                    ctx.Driver?.Quit();
                    ctx.Driver?.Dispose();
                }
                finally
                {
                    ctx.LoggerFactory?.Dispose();
                }
            }
            finally
            {
                _current.Value = null;
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
