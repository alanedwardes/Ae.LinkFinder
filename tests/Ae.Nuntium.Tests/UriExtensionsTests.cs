using Ae.Nuntium.Services;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class UriExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("data:image/png")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("#")]
        public void TryCreateInvalidUri(string invalidUri)
        {
            Assert.False(UriExtensions.TryCreateAbsoluteUri(invalidUri, new Uri("https://www.example.com/"), out var newUri));
            Assert.Null(newUri);
        }

        [Theory]
        [InlineData("/test", "https://www.example.com/", "https://www.example.com/test")]
        [InlineData("https://www.example.com/test", "https://www.example.net/", "https://www.example.com/test")]
        public void TryCreateValidUri(string url, string baseAddress, string result)
        {
            Assert.True(UriExtensions.TryCreateAbsoluteUri(url, new Uri(baseAddress), out var newUri));
            Assert.Equal(new Uri(result), newUri);
        }
    }
}
