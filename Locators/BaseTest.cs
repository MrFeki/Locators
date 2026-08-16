using Locators.Core.Configuration;
using Locators.Core.Diagnostics;
using Locators.Core.Logging;
using Locators.Core.WebDriver;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Tests;

public abstract class BaseTest
{
    private ILoggerFactory? loggerFactory;
    protected IWebDriver Driver { get; private set; } = null!;
    protected WebDriverWait Wait { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected string DownloadDirectory { get; private set; } = string.Empty;

    [SetUp]
    public void BaseSetUp()
    {
        var settings = TestConfiguration.Instance.Settings;
        var testName = TestContext.CurrentContext.Test.Name;
        var artifacts = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.WorkDirectory, settings.ArtifactsDirectory));
        DownloadDirectory = Path.Combine(artifacts, "downloads", Sanitize(testName));
        Directory.CreateDirectory(DownloadDirectory);
        Directory.CreateDirectory(Path.Combine(artifacts, "logs"));
        loggerFactory = LogFactory.Create(settings.MinimumLogLevel, Path.Combine(artifacts, "logs", $"{Sanitize(testName)}-.log"));
        Logger = loggerFactory.CreateLogger(GetType());
        Logger.LogInformation("START {Test}; environment={Environment}; browser={Browser}", testName, settings.ActiveEnvironment, settings.Browser);
        Driver = BrowserFactory.Create(settings, DownloadDirectory);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(settings.ExplicitWaitSeconds));
        Driver.Navigate().GoToUrl(settings.Environment.BaseUrl);
        Logger.LogInformation("Opened {Url}", settings.Environment.BaseUrl);
    }

    [TearDown]
    public void BaseTearDown()
    {
        var result = TestContext.CurrentContext.Result;
        try
        {
            if (result.Outcome.Status == TestStatus.Failed && Driver is not null)
            {
                var settings = TestConfiguration.Instance.Settings;
                var directory = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.WorkDirectory, settings.ArtifactsDirectory, "screenshots"));
                var screenshot = ScreenshotService.Capture(Driver, directory, TestContext.CurrentContext.Test.Name);
                TestContext.AddTestAttachment(screenshot, "Failure screenshot");
                Logger.LogError("FAILED: {Message}. Screenshot: {Screenshot}", result.Message, screenshot);
            }
            else Logger?.LogInformation("END with status {Status}", result.Outcome.Status);
        }
        catch (Exception exception) { Logger?.LogError(exception, "Could not capture failure screenshot."); }
        finally
        {
            Driver?.Quit();
            Driver?.Dispose();
            loggerFactory?.Dispose();
        }
    }

    private static string Sanitize(string value) => string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
