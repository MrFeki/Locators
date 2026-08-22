using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Locators.Features.Support;
using Locators.Pages;

namespace Locators.Features.Steps
{
    [Binding]
    public class CarouselSteps
    {
        private IWebDriver driver => TestContextHelper.Driver!;
        private ILogger logger => TestContextHelper.Logger!;

        private HomePage home => new HomePage(driver, TestContextHelper.Wait!, logger);
        private InsightsPage insights => new InsightsPage(driver, TestContextHelper.Wait!, logger);
        private ArticlePage article => new ArticlePage(driver, TestContextHelper.Wait!, logger);

        [When("I navigate to the Insights page")]
        public void WhenINavigateToTheInsightsPage()
        {
            home.ClickInsights();
        }

        [When("I wait for the Insights content")]
        public void WhenIWaitForTheInsightsContent()
        {
            insights.WaitForInsightsContent();
        }

        [When("I swipe the carousel (.*) times")]
        public void WhenISwipeTheCarouselTimes(int count)
        {
            insights.SwipeCarousel(count);
        }

        [When("I note the active carousel article title")]
        public void WhenINoteTheActiveCarouselArticleTitle()
        {
            var active = insights.GetActiveCarouselSlide();
            var title = insights.GetCarouselArticleTitle(active);
            ScenarioContext.Current["carouselTitle"] = title;
            ScenarioContext.Current["activeSlide"] = active;
        }

        [When("I open the article from the carousel")]
        public void WhenIOpenTheArticleFromTheCarousel()
        {
            var active = ScenarioContext.Current["activeSlide"] as IWebElement;
            if (active == null) throw new InvalidOperationException("Active carousel slide not found in scenario context");
            insights.ClickCarouselArticleLink(active);
        }

        [Then("the article page title should start with the noted carousel title")]
        public void ThenTheArticlePageTitleShouldStartWithTheNotedCarouselTitle()
        {
            var noted = ScenarioContext.Current["carouselTitle"] as string ?? string.Empty;
            var pageTitle = article.GetArticlePageTitle();
            if (string.IsNullOrWhiteSpace(pageTitle)) throw new InvalidOperationException("Article page title was not found");
            if (!pageTitle.StartsWith(noted, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException($"Article page title '{pageTitle}' does not match carousel title '{noted}'");
        }
    }
}
