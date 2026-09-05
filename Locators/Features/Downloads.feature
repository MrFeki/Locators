Feature: File downloads
  As a user
  I want to download files from the footer
  So that I can validate files are downloaded correctly

  Scenario Outline: Validate File Download
    Given I open the EPAM homepage
    When I scroll to the footer
    And I find the Code of Ethical Conduct link for "<ExpectedFile>"
    And I click the download link
    Then the file "<ExpectedFile>" should be downloaded

    Examples:
      | ExpectedFile                  |
      | Code_of_Ethical_Conduct.pdf   |
