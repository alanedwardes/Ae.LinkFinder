namespace Ae.Nuntium
{
    public sealed class ExceptionDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestDescription = $"GET {request.RequestUri}";

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Got exception when sending {requestDescription}.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                using (response)
                {
                    throw new HttpRequestException($"Response status code for {requestDescription} does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                }
            }

            return response;
        }
    }
}