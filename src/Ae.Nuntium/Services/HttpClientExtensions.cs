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
                var responseAsString = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Got status code {response.StatusCode} from {response.RequestMessage?.RequestUri}. Response: {responseAsString.Truncate(2048)}");
            }

            return response;
        }
    }
}
