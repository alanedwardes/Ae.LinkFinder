namespace Ae.Nuntium
{
    public sealed class ExceptionDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestDescription = $"GET {request.RequestUri}";
            try
            {
                using var response = await base.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Response status code for {requestDescription} does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Got exception when sending {requestDescription}.", ex);
            }
        }
    }
}