using System.Net.Http;
using System.Threading.Tasks;

namespace Locators.Core
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request);
        }

        public Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            return _httpClient.GetAsync(endpoint);
        }

        public Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content)
        {
            return _httpClient.PostAsync(endpoint, content);
        }
    }
}
