namespace Ae.Nuntium
{
    public sealed class GlobalTimeoutDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(request.Method == HttpMethod.Get ? 5 : 10);
            using var timeoutSource = new CancellationTokenSource(timeout);
            using var newSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
            try
            {
                return await base.SendAsync(request, newSource.Token);
            }
            catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
            {
                throw new TimeoutException($"Request timed out after {timeout} seconds");
            }
        }
    }
}