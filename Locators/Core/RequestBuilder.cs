using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Locators.Core
{
    public class RequestBuilder
    {
        private HttpMethod _method = HttpMethod.Get;
        private string _endpoint = string.Empty;
        private object? _body;
        private readonly HttpRequestMessage _message = new HttpRequestMessage();

        public static RequestBuilder Create() => new RequestBuilder();

        public RequestBuilder WithMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }

        public RequestBuilder WithEndpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }

        public RequestBuilder WithJsonBody(object body)
        {
            _body = body;
            return this;
        }

        public RequestBuilder WithHeader(string name, string value)
        {
            _message.Headers.Remove(name);
            _message.Headers.Add(name, value);
            return this;
        }

        public HttpRequestMessage Build()
        {
            _message.Method = _method;
            _message.RequestUri = new System.Uri(_endpoint, System.UriKind.RelativeOrAbsolute);
            if (_body != null)
            {
                var json = JsonSerializer.Serialize(_body);
                _message.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return _message;
        }
    }
}
