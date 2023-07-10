using Ae.Nuntium.Services;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class UriExtensionsTests
    {
        [Fact]
        public void TryCreateAbsoluteUriNull()
        {
            Assert.False(UriExtensions.TryCreateAbsoluteUri(null, new Uri("https://www.example.com/"), out var newUri));
            Assert.Null(newUri);
        }

        [Fact]
        public void TryCreateAbsoluteUriFromRelative()
        {
            Assert.True(UriExtensions.TryCreateAbsoluteUri("/test", new Uri("https://www.example.com/"), out var newUri));
            Assert.Equal(new Uri("https://www.example.com/test"), newUri);
        }

        [Fact]
        public void TryCreateAbsoluteUri()
        {
            Assert.True(UriExtensions.TryCreateAbsoluteUri("https://www.example.com/test", new Uri("https://www.example.net/"), out var newUri));
            Assert.Equal(new Uri("https://www.example.com/test"), newUri);
        }
    }
}
