using RestSharp;
using Serilog;

namespace Locators.Core
{
    public class ApiClient : IDisposable
    {
        private readonly RestClient _client;

        public ApiClient(string baseUrl)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException("Base API URL must be a valid absolute URL.", nameof(baseUrl));

            _client = new RestClient(new RestClientOptions(baseUri));
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            LogRequest(request);
            var response = await _client.ExecuteAsync(request);
            LogResponse(request, response);
            return response;
        }

        public async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request)
        {
            LogRequest(request);
            var response = await _client.ExecuteAsync<T>(request);
            LogResponse(request, response);
            return response;
        }

        private static void LogRequest(RestRequest request)
        {
            Log.Information("API request: {Method} {Resource}", request.Method, request.Resource);
        }

        private static void LogResponse(RestRequest request, RestResponse response)
        {
            if (response.IsSuccessful)
            {
                Log.Information("API response: {Method} {Resource} -> {StatusCode}",
                    request.Method, request.Resource, (int)response.StatusCode);
                return;
            }

            Log.Error("API response failed: {Method} {Resource} -> {StatusCode}. Error: {Error}. Body: {Body}",
                request.Method, request.Resource, (int)response.StatusCode,
                response.ErrorMessage, response.Content);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
