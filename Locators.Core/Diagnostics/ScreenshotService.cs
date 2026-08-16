using OpenQA.Selenium;

namespace Locators.Core.Diagnostics;

public static class ScreenshotService
{
    public static string Capture(IWebDriver driver, string directory, string testName)
    {
        Directory.CreateDirectory(directory);
        var safeName = string.Concat(testName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var path = Path.Combine(directory, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
        if (driver is not ITakesScreenshot camera)
            throw new NotSupportedException("The active driver cannot capture screenshots.");
        camera.GetScreenshot().SaveAsFile(path);
        return path;
    }
}
