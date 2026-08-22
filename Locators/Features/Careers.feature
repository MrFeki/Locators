Feature: Careers search
  As a user
  I want to search for jobs on the Careers page
  So that I can verify search results contain the requested programming language

  Scenario Outline: Validate Search Position
    Given I open the EPAM homepage
    When I navigate to the Careers page
    And I start the job search
    And I search for "<Language>"
    And I choose country "<Country>"
    And I set the Remote checkbox
    And I submit the search
    Then the latest job should contain "<Language>"

    Examples:
      | Language | Country |
      | Python   | Mexico  |
      | Java     | Serbia  |
      | .NET     | Ukraine |
