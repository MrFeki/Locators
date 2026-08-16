using Locators.Core.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace Locators.Core.WebDriver;

public static class BrowserFactory
{
    public static IWebDriver Create(TestSettings settings, string downloadDirectory)
    {
        IWebDriver driver = settings.Browser.Trim().ToLowerInvariant() switch
        {
            "chrome" => CreateChrome(settings, downloadDirectory),
            "edge" => CreateEdge(settings, downloadDirectory),
            "firefox" => CreateFirefox(settings, downloadDirectory),
            _ => throw new NotSupportedException($"Browser '{settings.Browser}' is not supported.")
        };

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        if (!settings.Headless) driver.Manage().Window.Maximize();
        return driver;
    }

    private static ChromeDriver CreateChrome(TestSettings settings, string downloadDirectory)
    {
        var options = new ChromeOptions();
        options.AddUserProfilePreference("download.default_directory", downloadDirectory);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
        options.AddArgument("--disable-notifications");
        if (settings.Headless) options.AddArgument("--headless=new");
        return new ChromeDriver(options);
    }

    private static EdgeDriver CreateEdge(TestSettings settings, string downloadDirectory)
    {
        var options = new EdgeOptions();
        options.AddUserProfilePreference("download.default_directory", downloadDirectory);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        if (settings.Headless) options.AddArgument("--headless=new");
        return new EdgeDriver(options);
    }

    private static FirefoxDriver CreateFirefox(TestSettings settings, string downloadDirectory)
    {
        var options = new FirefoxOptions();
        options.SetPreference("browser.download.dir", downloadDirectory);
        options.SetPreference("browser.download.folderList", 2);
        options.SetPreference("pdfjs.disabled", true);
        if (settings.Headless) options.AddArgument("-headless");
        return new FirefoxDriver(options);
    }
}
