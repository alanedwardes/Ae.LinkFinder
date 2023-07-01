namespace Ae.Nuntium.Tests
{
    public sealed class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly MockDelegatingHandler _mockHandler;

        private sealed class MockDelegatingHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _mockResponder;

            public MockDelegatingHandler(Func<HttpRequestMessage, HttpResponseMessage> mockResponder) => _mockResponder = mockResponder;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_mockResponder(request));
        }

        public MockHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> mockResponder) => _mockHandler = new MockDelegatingHandler(mockResponder);

        public HttpClient CreateClient(string name) => new HttpClient(_mockHandler);
    }
}
