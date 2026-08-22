using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Locators.Features.Support;
using Locators.Pages;

namespace Locators.Features.Steps
{
    [Binding]
    public class DownloadsSteps
    {
        private IWebDriver driver => TestContextHelper.Driver!;
        private ILogger logger => TestContextHelper.Logger!;

        private HomePage home => new HomePage(driver, TestContextHelper.Wait!, logger);

        [When("I scroll to the footer")]
        public void WhenIScrollToTheFooter()
        {
            home.ScrollToFooter();
        }

        [When("I find the Code of Ethical Conduct link for \"(.*)\"")]
        public void WhenIFindTheCodeOfEthicalConductLinkFor(string expectedFile)
        {
            // ensure page object knows download directory configured in TestContextHelper
            home.DownloadDirectory = TestContextHelper.DownloadDirectory;

            var (element, fileName) = home.FindCodeOfConductLink(expectedFile);
            ScenarioContext.Current["downloadElement"] = element;
            ScenarioContext.Current["downloadFileName"] = string.IsNullOrWhiteSpace(fileName) ? expectedFile : fileName;
        }

        [When("I click the download link")]
        public void WhenIClickTheDownloadLink()
        {
            var element = ScenarioContext.Current["downloadElement"] as IWebElement;
            if (element == null) throw new InvalidOperationException("Download link element not found in scenario context");

            // capture existing files before initiating download
            var before = System.IO.Directory.GetFiles(TestContextHelper.DownloadDirectory).Select(System.IO.Path.GetFileName).ToHashSet();
            ScenarioContext.Current["downloadExistingFiles"] = before;

            home.ClickDownloadLink(element);
        }

        [Then("the file \"(.*)\" should be downloaded")]
        public void ThenTheFileShouldBeDownloaded(string expectedFile)
        {
            var expectedFileNameHint = ScenarioContext.Current["downloadFileName"] as string ?? expectedFile;
            var before = ScenarioContext.Current["downloadExistingFiles"] as System.Collections.Generic.HashSet<string> ?? new System.Collections.Generic.HashSet<string>();
            string downloadDir = TestContextHelper.DownloadDirectory;

            // wait until a new non-temporary file appears in the download directory
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string? found = null;
            var timeout = TimeSpan.FromSeconds(30);

            while (sw.Elapsed < timeout)
            {
                var files = System.IO.Directory.GetFiles(downloadDir);
                var candidate = files
                    .Where(f => !f.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase))
                    .Select(f => new System.IO.FileInfo(f))
                    .Where(fi => fi.Length > 0)
                    .Select(fi => fi.Name)
                    .FirstOrDefault(name => !before.Contains(name));

                if (candidate != null)
                {
                    found = candidate;
                    break;
                }

                System.Threading.Thread.Sleep(500);
            }

            if (found is null)
            {
                throw new InvalidOperationException($"Expected a new downloaded file to appear in '{downloadDir}' within the timeout. Hint expected name: '{expectedFileNameHint}'.");
            }

            var path = System.IO.Path.Combine(downloadDir, found);

            // final checks
            if (!System.IO.File.Exists(path)) throw new InvalidOperationException($"Downloaded file '{found}' does not exist at '{path}'.");
            long fileSize = new System.IO.FileInfo(path).Length;
            if (fileSize == 0) throw new InvalidOperationException($"Downloaded file '{found}' at '{path}' is empty.");

            // store actual downloaded filename for debugging if needed
            ScenarioContext.Current["downloadedFileName"] = found;
        }
    }
}
