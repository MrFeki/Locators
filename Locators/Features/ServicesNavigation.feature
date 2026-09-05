Feature: Services navigation
  As a user
  I want to navigate to a specific service category from the Services menu
  So that I can verify the category page title and related expertise section

  @services
  Scenario Outline: Validate Navigation to Services Section
    Given I open the website homepage
    When I hover over the Services link
    And I select the "<Category>" service from the dropdown
    Then I should see a page title containing "<ExpectedTitle>"
    And the "Our Related Expertise" section should be visible

    Examples:
      | Category       | ExpectedTitle  |
      | Generative AI  | Generative AI  |
      | Responsible AI | Responsible AI |
