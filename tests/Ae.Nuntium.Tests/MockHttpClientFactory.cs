namespace Ae.Nuntium.Tests
{
    public sealed class MockHttpClientFactory : IHttpClientFactory
    {
        private sealed class MockDelegatingHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _mockResponder;

            public MockDelegatingHandler(Func<HttpRequestMessage, HttpResponseMessage> mockResponder) => _mockResponder = mockResponder;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_mockResponder(request));
        }

        private readonly IList<DelegatingHandler> _createdHandlers = new List<DelegatingHandler>();

        public Func<HttpRequestMessage, HttpResponseMessage> MockResponder { get; }

        public DelegatingHandler CreateHandler()
        {
            var handler = new ExceptionDelegatingHandler { InnerHandler = new MockDelegatingHandler(MockResponder) };
            _createdHandlers.Add(handler);
            return handler;
        }

        public MockHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> mockResponder) => MockResponder = mockResponder;

        public HttpClient CreateClient(string name) => new(CreateHandler());
    }
}
