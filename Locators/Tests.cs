using Locators.Pages;
using Microsoft.Extensions.Logging;

namespace Locators.Tests;

public sealed class Tests : BaseTest
{
    private static readonly string[] SearchKeywords = ["BLOCKCHAIN", "Cloud", "Automation"];
    private HomePage Home => new(Driver, Wait, Logger) { DownloadDirectory = DownloadDirectory };
    private CareersPage Careers => new(Driver, Wait, Logger);
    private GlobalSearchPage GlobalSearch => new(Driver, Wait, Logger);
    private InsightsPage Insights => new(Driver, Wait, Logger);
    private ArticlePage Article => new(Driver, Wait, Logger);

    [TestCase("Python", "Mexico")]
    [TestCase("Java", "Serbia")]
    [TestCase(".NET", "Ukraine")]
    public void ValidateSearchPosition(string programmingLanguage, string country)
    {
        Logger.LogInformation("Search for {Language} positions in {Country}", programmingLanguage, country);
        Home.ClickCareers(); Careers.ClickStartYourSearch();
        Careers.SearchForProgrammingLanguage(programmingLanguage); Careers.ChooseCountry(country);
        Careers.SetCheckboxRemote(); Careers.ClickSubmitButton(); Careers.WaitForJobResults();
        Careers.ValidateLatestJobContainsLanguage(programmingLanguage);
    }

    [TestCaseSource(nameof(SearchKeywords))]
    public void ValidateGlobalSearch(string query)
    {
        Logger.LogInformation("Run global search for {Query}", query);
        Home.ClickSearchIcon(); GlobalSearch.EnterGlobalSearchQuery(Home.GetGlobalSearchInput(), query);
        GlobalSearch.ClickGlobalFindButton(); GlobalSearch.WaitForGlobalSearchResults();
        GlobalSearch.ValidateAllLinksContainWord(query);
    }

    [TestCase("Code-Of-Conduct_01_26.pdf")]
    public void ValidateFileDownload(string expectedFileName)
    {
        var (codeLink, actualFileName) = Home.FindCodeOfConductLink(expectedFileName);
        var expectedFilePath = Path.Combine(DownloadDirectory, actualFileName);
        if (File.Exists(expectedFilePath)) File.Delete(expectedFilePath);
        Home.ScrollToFooter();
        Home.ClickDownloadLink(codeLink);
        Assert.Multiple(() =>
        {
            Assert.That(Home.WaitForFileDownload(expectedFilePath), Is.True, "Download did not complete.");
            Assert.That(new FileInfo(expectedFilePath).Length, Is.GreaterThan(0), "Downloaded file is empty.");
        });
        Logger.LogInformation("Downloaded {File} to {Path}", actualFileName, expectedFilePath);
    }

    [Test]
    public void ValidateCarouselArticleTitle()
    {
        Home.ClickInsights(); Insights.WaitForInsightsContent(); Insights.SwipeCarousel(3);
        var activeSlide = Insights.GetActiveCarouselSlide();
        var carouselTitle = Insights.GetCarouselArticleTitle(activeSlide);
        Insights.ClickCarouselArticleLink(activeSlide);
        var articleTitle = Article.GetArticlePageTitle();
        Assert.That(articleTitle, Does.StartWith(carouselTitle).IgnoreCase,
            $"Article title '{articleTitle}' does not match carousel title '{carouselTitle}'.");
        Logger.LogInformation("Article title matches carousel title: {Title}", carouselTitle);
    }
}
