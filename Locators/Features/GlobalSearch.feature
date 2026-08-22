Feature: Global search
  As a user
  I want to perform a global search from the header
  So that I can validate search results contain the searched term

  Scenario Outline: Validate Global Search
    Given I open the EPAM homepage
    When I open the global search input
    And I search for "<Query>"
    Then all search result links should contain "<Query>"

    Examples:
      | Query      |
      | BLOCKCHAIN |
      | Cloud      |
      | Automation |
