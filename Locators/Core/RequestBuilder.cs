using RestSharp;

namespace Locators.Core
{
    public class RequestBuilder
    {
        private Method _method = Method.Get;
        private string _endpoint = string.Empty;
        private object? _body;
        private readonly Dictionary<string, string> _headers = new();

        private RequestBuilder() { }

        public static RequestBuilder Create() => new();

        public RequestBuilder WithMethod(Method method)
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
            _headers[name] = value;
            return this;
        }

        public RestRequest Build()
        {
            if (string.IsNullOrWhiteSpace(_endpoint))
                throw new InvalidOperationException("Request endpoint must be specified.");

            var request = new RestRequest(_endpoint, _method);

            foreach (var header in _headers)
                request.AddOrUpdateHeader(header.Key, header.Value);

            if (_body is not null)
                request.AddJsonBody(_body);

            return request;
        }
    }
}
