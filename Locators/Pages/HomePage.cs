using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Locators.Pages
{
    public class HomePage : BasePage
    {
        public string DownloadDirectory { get; set; } = string.Empty;

        public HomePage(IWebDriver driver, WebDriverWait wait, ILogger logger)
            : base(driver, wait, logger)
        {
        }

        public void ClickCareers()
        {
            logger.LogInformation("Finding Careers link.");
            var careersLink = WaitForVisibleAndEnabled(By.LinkText("Careers"));
            careersLink.Click();
            logger.LogInformation("Careers link clicked.");
        }

        public void ClickInsights()
        {
            logger.LogInformation("Finding Insights link.");
            var insightsLink = WaitForVisibleAndEnabled(By.LinkText("Insights"));
            insightsLink.Click();
            logger.LogInformation("Insights link clicked.");
        }

        public void ClickSearchIcon()
        {
            var searchIcon = WaitForVisibleAndEnabled(By.XPath("//span[contains(@class,'header-search__search-icon')]") );
            searchIcon.Click();
            logger.LogInformation("Global search icon clicked.");
        }

        public IWebElement GetGlobalSearchInput()
        {
            return WaitForVisibleAndEnabled(By.Id("new_form_search"));
        }

        public void ScrollToFooter()
        {
            logger.LogInformation("Jumping to the bottom of the page.");
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            logger.LogInformation("Page bottom reached.");
        }

        public IWebElement FindCodeOfConductLink(string expectedFileName)
        {
            logger.LogInformation("Scrolling up until Code of Ethical Conduct link is visible.");

            var downloadWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            By linkLocator = By.LinkText("Code of Ethical Conduct (PDF)");

            IWebElement? codeLink = downloadWait.Until(d =>
            {
                try
                {
                    IWebElement? link = d.FindElements(linkLocator).FirstOrDefault();

                    if (link is not null)
                    {
                        ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].scrollIntoView({block:'center', inline:'nearest'});", link);

                        bool isInsideViewport = (bool)((IJavaScriptExecutor)d).ExecuteScript(@"
                                const rect = arguments[0].getBoundingClientRect();
                                return (rect.bottom > 0 && rect.top < window.innerHeight);
                                ", link);

                        if (isInsideViewport)
                        {
                            logger.LogInformation("Code of Ethical Conduct link entered the visible viewport.");
                            return link;
                        }

                        ((IJavaScriptExecutor)d).ExecuteScript(@"
                                function findScrollable(el){
                                    while(el && el !== document.body){
                                        const style = window.getComputedStyle(el);
                                        if (/(auto|scroll)/.test(style.overflow + style.overflowY)) return el;
                                        el = el.parentElement;
                                    }
                                    return document.documentElement || document.body;
                                }

                                var sc = findScrollable(arguments[0] || document.body);
                                sc.scrollTop = Math.max(0, sc.scrollTop - 200);
                                ", link);
                    }

                    ((IJavaScriptExecutor)d).ExecuteScript("window.scrollBy(0, -50);");
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });

            Assert.That(codeLink, Is.Not.Null, $"Could not find Code of Ethical Conduct link for '{expectedFileName}'.");

            string href = codeLink!.GetAttribute("href") ?? string.Empty;

            Assert.That(href, Does.Contain(expectedFileName), $"Code of Ethical Conduct link does not point to '{expectedFileName}'.");

            logger.LogInformation("Code of Ethical Conduct link is visible. Href: {Href}", href);

            return codeLink;
        }

        public void ClickDownloadLink(IWebElement codeLink)
        {
            logger.LogInformation("Clicking Code of Ethical Conduct PDF link.");
            codeLink.Click();
            logger.LogInformation("Code of Ethical Conduct PDF link clicked.");
        }

        public bool WaitForFileDownload(string expectedFilePath)
        {
            logger.LogInformation("Waiting for file download.");

            var downloadWait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            bool downloaded = downloadWait.Until(d =>
            {
                bool expectedFileExists = File.Exists(expectedFilePath);
                bool temporaryDownloadExists = Directory.GetFiles(DownloadDirectory, "*.crdownload").Any();

                if (!expectedFileExists || temporaryDownloadExists) return false;

                long fileSize = new FileInfo(expectedFilePath).Length;
                return fileSize > 0;
            });

            logger.LogInformation("File download completed.");

            return downloaded;
        }
    }
}
