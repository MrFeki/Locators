using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Locators.Core;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Locators.Business.Models;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Locators.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ApiTests
    {
        private static ApiClient? _client;
        private static string? _baseApiUrl;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {

            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var jsonPath = Path.Combine(projectDir, "appsettings.json");

            IConfigurationRoot config;

            if (File.Exists(jsonPath))
            {
                var text = File.ReadAllText(jsonPath);
                try
                {
                    JsonDocument.Parse(text);

                    config = new ConfigurationBuilder()
                        .SetBasePath(projectDir)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .Build();
                }
                catch (JsonException)
                {
                    var defaults = new Dictionary<string, string?>
                    {
                        ["BaseApiUrl"] = "https://jsonplaceholder.typicode.com/",
                        ["Logging:MinLevel"] = "Information",
                        ["Logging:FilePath"] = "logs/test.log"
                    };
                    config = new ConfigurationBuilder()
                        .AddInMemoryCollection(defaults)
                        .Build();
                }
            }
            else
            {
                var defaults = new Dictionary<string, string?>
                {
                    ["BaseApiUrl"] = "https://jsonplaceholder.typicode.com/",
                    ["Logging:MinLevel"] = "Information",
                    ["Logging:FilePath"] = "logs/test.log"
                };
                config = new ConfigurationBuilder()
                    .AddInMemoryCollection(defaults)
                    .Build();
            }

            _baseApiUrl = config["BaseApiUrl"] ?? "https://jsonplaceholder.typicode.com/";

            var minLevel = config["Logging:MinLevel"] ?? "Information";
            var filePath = config["Logging:FilePath"] ?? "logs/test.log";

            var level = Serilog.Events.LogEventLevel.Information;
            if (!System.Enum.TryParse<Serilog.Events.LogEventLevel>(minLevel, true, out level))
            {
                level = Serilog.Events.LogEventLevel.Information;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console()
                .WriteTo.File(filePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Initializing HttpClient with base url {BaseUrl}", _baseApiUrl);

            var httpClient = new HttpClient { BaseAddress = new System.Uri(_baseApiUrl) };
            _client = new ApiClient(httpClient);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Log.Information("Tests finished");
            Log.CloseAndFlush();
        }

        [Test]
        [Category("API")]
        public async Task GetUsers_ListReturned_WithExpectedFields()
        {
            Log.Information("Test: GetUsers_ListReturned_WithExpectedFields - Sending GET /users");

            var request = RequestBuilder.Create().WithMethod(HttpMethod.Get).WithEndpoint("users").Build();
            var response = await _client!.SendAsync(request);

            Log.Information("Requested: {Uri}", response.RequestMessage.RequestUri);
            Assert.AreEqual(new System.Uri(_baseApiUrl + "users"), response.RequestMessage.RequestUri);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                Log.Error("Unexpected response {Status} for {Uri}. Body: {Body}", response.StatusCode, response.RequestMessage.RequestUri, body);
                Log.Information("Status: {Status}, Body: {Body}", response.StatusCode, body);
            }

            Log.Information("Received response {StatusCode}", response.StatusCode);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, "Expected 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrEmpty(content), "Response content should not be empty");

            var users = JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(users, "Users should deserialize");
            Assert.IsTrue(users!.Count > 0, "Users list should not be empty");

            foreach (var u in users)
            {
                Assert.That(u.Id, Is.GreaterThan(0)); 
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Name), "User Name should not be empty");
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Username), "User Username should not be empty");
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Email), "User Email should not be empty");
                Assert.IsNotNull(u.Address, "User Address should not be null");
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Phone), "User Phone should not be empty");
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Website), "User Website should not be empty");
                Assert.IsNotNull(u.Company, "User Company should not be null");
            }

            Log.Information("Validated presence of required user fields");
        }

        [Test]
        [Category("API")]
        public async Task GetUsers_ResponseHasContentTypeHeader()
        {
            Log.Information("Test: GetUsers_ResponseHasContentTypeHeader - Sending GET /users");

            var request = RequestBuilder.Create().WithMethod(HttpMethod.Get).WithEndpoint("users").Build();
            var response = await _client!.SendAsync(request);

            Log.Information("Requested: {Uri}", response.RequestMessage.RequestUri);
            Assert.AreEqual(new System.Uri(_baseApiUrl + "users"), response.RequestMessage.RequestUri);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                Log.Error("Unexpected response {Status} for {Uri}. Body: {Body}", response.StatusCode, response.RequestMessage.RequestUri, body);
                Log.Information("Status: {Status}, Body: {Body}", response.StatusCode, body);
            }

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, "Expected 200 OK");

            var hasContentType = response.Content.Headers.ContentType != null;
            Assert.IsTrue(hasContentType, "Content-Type header should exist");

            var contentType = response.Content.Headers.ContentType!.ToString();
            Log.Information("Content-Type header value: {ContentType}", contentType);
            Assert.AreEqual("application/json; charset=utf-8", contentType.ToLower(), "Unexpected content type");
        }

        [Test]
        [Category("API")]
        public async Task GetUsers_ValidateCollectionIntegrity()
        {
            Log.Information("Test: GetUsers_ValidateCollectionIntegrity - Sending GET /users");

            var request = RequestBuilder.Create().WithMethod(HttpMethod.Get).WithEndpoint("users").Build();
            var response = await _client!.SendAsync(request);

            Log.Information("Requested: {Uri}", response.RequestMessage.RequestUri);
            Assert.AreEqual(new System.Uri(_baseApiUrl + "users"), response.RequestMessage.RequestUri);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                Log.Error("Unexpected response {Status} for {Uri}. Body: {Body}", response.StatusCode, response.RequestMessage.RequestUri, body);
                Log.Information("Status: {Status}, Body: {Body}", response.StatusCode, body);
            }

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode, "Expected 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(users, "Users should deserialize");

            Assert.AreEqual(10, users!.Count, "Expected array of 10 users");

            var idsCount = users.Select(u => u.Id).Distinct().Count();
            Assert.AreEqual(10, idsCount, "Each user should have unique ID");

            foreach (var u in users)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Name), "User name should not be empty");
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Username), "User username should not be empty");
                Assert.IsNotNull(u.Company);
                Assert.IsFalse(string.IsNullOrWhiteSpace(u.Company!.Name), "Company name should not be empty");
            }

            Log.Information("Validated users collection integrity");
        }

        [Test]
        [Category("API")]
        public async Task CreateUser_PostCreatesUser_Returns201AndId()
        {
            Log.Information("Test: CreateUser_PostCreatesUser_Returns201AndId - Sending POST /users");

            var payload = new Locators.Business.Models.CreateUserRequest { Name = "Test User", Username = "testuser" };
            var request = RequestBuilder.Create().WithMethod(HttpMethod.Post).WithEndpoint("users").WithJsonBody(payload).Build();
            var response = await _client!.SendAsync(request);

            Log.Information("Requested: {Uri}", response.RequestMessage.RequestUri);
            Assert.AreEqual(new System.Uri(_baseApiUrl + "users"), response.RequestMessage.RequestUri);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var body = await response.Content.ReadAsStringAsync();
                Log.Error("Unexpected response {Status} for {Uri}. Body: {Body}", response.StatusCode, response.RequestMessage.RequestUri, body);
                Log.Information("Status: {Status}, Body: {Body}", response.StatusCode, body);
            }

            Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode, "Expected 201 Created");

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(content), "Response should not be empty");

            var created = JsonSerializer.Deserialize<Locators.Business.Models.CreateUserResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(created, "Response should deserialize to CreateUserResponse");
            Assert.IsTrue(created!.Id.HasValue && created.Id.Value > 0, "Id value should be present and > 0");

            if (!string.IsNullOrWhiteSpace(created.Name))
            {
                Assert.AreEqual("Test User", created.Name, "Returned name should match payload");
            }
            if (!string.IsNullOrWhiteSpace(created.Username))
            {
                Assert.AreEqual("testuser", created.Username, "Returned username should match payload");
            }

            Log.Information("User created with id {Id}", created.Id);
        }

        [Test]
        [Category("API")]
        public async Task InvalidEndpoint_Returns404()
        {
            Log.Information("Test: InvalidEndpoint_Returns404 - Sending GET /invalidendpoint");

            var request = RequestBuilder.Create().WithMethod(HttpMethod.Get).WithEndpoint("invalidendpoint").Build();
            var response = await _client!.SendAsync(request);

            Log.Information("Requested: {Uri}", response.RequestMessage.RequestUri);
            Assert.AreEqual(new System.Uri(_baseApiUrl + "invalidendpoint"), response.RequestMessage.RequestUri);

            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var body = await response.Content.ReadAsStringAsync();
                Log.Error("Unexpected response {Status} for {Uri}. Body: {Body}", response.StatusCode, response.RequestMessage.RequestUri, body);
                Log.Information("Status: {Status}, Body: {Body}", response.StatusCode, body);
            }

            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode, "Expected 404 Not Found");

            Log.Information("Received expected 404 for invalid endpoint");
        }
    }
}
