Feature: Carousel article
  As a user
  I want to inspect the Insights carousel
  So that I can validate article title matches the article page

  Scenario: Validate Carousel Article Title
    Given I open the EPAM homepage
    When I navigate to the Insights page
    And I wait for the Insights content
    And I swipe the carousel 3 times
    And I note the active carousel article title
    And I open the article from the carousel
    Then the article page title should start with the noted carousel title
