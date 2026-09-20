using Locators.Business.Models;
using Locators.Core;
using NUnit.Framework;
using RestSharp;
using Serilog;
using System.Net;

namespace Locators.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ApiTests
    {
        private static ApiClient? _client;
        private static string _baseApiUrl = string.Empty;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var config = ApiTestHelpers.LoadConfiguration();
            ApiTestHelpers.ConfigureLogging(config);

            _baseApiUrl = config["BaseApiUrl"]
                ?? throw new InvalidOperationException("BaseApiUrl is missing from appsettings.json.");

            if (!Uri.TryCreate(_baseApiUrl, UriKind.Absolute, out _))
                throw new InvalidOperationException($"BaseApiUrl is invalid: {_baseApiUrl}");

            _client = new ApiClient(_baseApiUrl);
            Log.Information("API tests initialized with base URL {BaseUrl}", _baseApiUrl);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client?.Dispose();
            Log.Information("API tests finished");
            Log.CloseAndFlush();
        }

        [Test]
        [Category("API")]
        public async Task GetUsers_ListReturned_WithExpectedFields()
        {
            var request = RequestBuilder.Create()
                .WithMethod(Method.Get)
                .WithEndpoint("users")
                .Build();

            var response = await _client!.ExecuteAsync<List<User>>(request);

            ApiTestHelpers.AssertResponse(response, HttpStatusCode.OK);
            ApiTestHelpers.AssertResponseUri(response, _baseApiUrl, "users");

            Assert.That(response.Data, Is.Not.Null, "Users should deserialize.");
            Assert.That(response.Data, Is.Not.Empty, "Users list should not be empty.");

            foreach (var user in response.Data!)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(user.Id, Is.GreaterThan(0));
                    Assert.That(user.Name, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Username, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Email, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Phone, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Website, Is.Not.Null.And.Not.Empty);

                    Assert.That(user.Address, Is.Not.Null);
                    Assert.That(user.Address!.Street, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Address.City, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Address.Zipcode, Is.Not.Null.And.Not.Empty);

                    Assert.That(user.Company, Is.Not.Null);
                    Assert.That(user.Company!.Name, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Company.CatchPhrase, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Company.Bs, Is.Not.Null.And.Not.Empty);
                });
            }
        }

        [Test]
        [Category("API")]
        public async Task GetUsers_ResponseHasContentTypeHeader()
        {
            var request = RequestBuilder.Create()
                .WithMethod(Method.Get)
                .WithEndpoint("users")
                .Build();

            var response = await _client!.ExecuteAsync(request);

            ApiTestHelpers.AssertResponse(response, HttpStatusCode.OK);
            ApiTestHelpers.AssertResponseUri(response, _baseApiUrl, "users");

            Assert.Multiple(() =>
            {
                Assert.That(response.ContentType, Is.Not.Null.And.Not.Empty, "Content-Type should exist.");
                Assert.That(response.ContentType, Does.StartWith("application/json").IgnoreCase,
                    "Media type should be application/json.");
                Assert.That(response.ContentType, Does.Contain("charset=utf-8").IgnoreCase,
                    "Charset should be utf-8.");
            });
        }

        [Test]
        [Category("API")]
        public async Task GetUsers_ValidateCollectionIntegrity()
        {
            var request = RequestBuilder.Create()
                .WithMethod(Method.Get)
                .WithEndpoint("users")
                .Build();

            var response = await _client!.ExecuteAsync<List<User>>(request);

            ApiTestHelpers.AssertResponse(response, HttpStatusCode.OK);
            ApiTestHelpers.AssertResponseUri(response, _baseApiUrl, "users");

            var users = response.Data;
            Assert.That(users, Is.Not.Null);
            Assert.That(users, Has.Count.EqualTo(10), "Expected array of 10 users.");
            Assert.That(users!.Select(u => u.Id).Distinct().Count(), Is.EqualTo(10),
                "Each user should have a unique ID.");

            foreach (var user in users)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(user.Name, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Username, Is.Not.Null.And.Not.Empty);
                    Assert.That(user.Company, Is.Not.Null);
                    Assert.That(user.Company!.Name, Is.Not.Null.And.Not.Empty);
                });
            }
        }

        [Test]
        [Category("API")]
        public async Task CreateUser_PostCreatesUser_Returns201AndId()
        {
            var payload = new CreateUserRequest
            {
                Name = "Test User",
                Username = "testuser"
            };

            var request = RequestBuilder.Create()
                .WithMethod(Method.Post)
                .WithEndpoint("users")
                .WithHeader("Accept", "application/json")
                .WithJsonBody(payload)
                .Build();

            var response = await _client!.ExecuteAsync<CreateUserResponse>(request);

            ApiTestHelpers.AssertResponse(response, HttpStatusCode.Created);
            ApiTestHelpers.AssertResponseUri(response, _baseApiUrl, "users");

            var created = response.Data;
            Assert.That(created, Is.Not.Null, "Response should deserialize to CreateUserResponse.");

            Assert.Multiple(() =>
            {
                Assert.That(created!.Id, Is.Not.Null.And.GreaterThan(0));
                Assert.That(created.Name, Is.EqualTo(payload.Name), "Returned name should match payload.");
                Assert.That(created.Username, Is.EqualTo(payload.Username), "Returned username should match payload.");
            });
        }

        [Test]
        [Category("API")]
        public async Task InvalidEndpoint_Returns404()
        {
            var request = RequestBuilder.Create()
                .WithMethod(Method.Get)
                .WithEndpoint("invalidendpoint")
                .Build();

            var response = await _client!.ExecuteAsync(request);

            ApiTestHelpers.AssertResponse(response, HttpStatusCode.NotFound);
            ApiTestHelpers.AssertResponseUri(response, _baseApiUrl, "invalidendpoint");
        }
    }
}
