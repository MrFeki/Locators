using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Locators.Tests.Helpers
{
    public static class ApiResponseAssertions
    {
        public static void AssertResponse(RestResponse response, HttpStatusCode expectedStatusCode)
        {
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode),
                    $"Expected {(int)expectedStatusCode} {expectedStatusCode}. Body: {response.Content}");
                Assert.That(response.ResponseStatus, Is.EqualTo(ResponseStatus.Completed),
                    "The request should complete without transport or deserialization errors.");
            });
        }

        public static void AssertNoErrors(RestResponse response)
        {
            Assert.Multiple(() =>
            {
                Assert.That(response.ErrorMessage, Is.Null.Or.Empty,
                    "RestSharp should not report an error message.");
                Assert.That(response.ErrorException, Is.Null,
                    "RestSharp should not report an exception.");
            });
        }
    }
}
