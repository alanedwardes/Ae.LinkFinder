namespace Ae.Nuntium
{
    public sealed class GlobalTimeoutDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var newSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
            try
            {
                return await base.SendAsync(request, newSource.Token);
            }
            catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
            {
                throw new TimeoutException("Request timed out after 5 seconds");
            }
        }
    }
}