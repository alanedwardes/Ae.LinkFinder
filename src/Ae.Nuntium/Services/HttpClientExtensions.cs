using Humanizer;

namespace Ae.Nuntium.Services
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendWrapped(Task<HttpResponseMessage> requestTask)
        {
            var response = await requestTask;
            if (!response.IsSuccessStatusCode)
            {
                var requestAsString = await (response.RequestMessage?.Content?.ReadAsStringAsync() ?? Task.FromResult<string>(null));
                var responseAsString = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Got status code {response.StatusCode} from {response.RequestMessage?.RequestUri}." +
                    $"\nRequest: {requestAsString.Truncate(2048)}" +
                    $"\nResponse: {responseAsString.Truncate(2048)}");
            }

            return response;
        }
    }
}
