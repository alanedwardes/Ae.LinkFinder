using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class ExtractedPostExtensionsTests
    {
        [Fact]
        public void TestExtractedPostNoContent()
        {
            Assert.False(new ExtractedPost().HasContent());
        }

        [Fact]
        public void TestExtractedPostNoContentWithRawContent()
        {
            Assert.False(new ExtractedPost { RawContent = "<html>" }.HasContent());
        }

        [Fact]
        public void TestExtractedPostHasContentWithPermalink()
        {
            Assert.True(new ExtractedPost { Permalink = new Uri("https://example.com") }.HasContent());
        }

        [Fact]
        public void TestExtractedPostHasContentWithTitle()
        {
            Assert.True(new ExtractedPost { Title = "title" }.HasContent());
        }

        [Fact]
        public void TestExtractedPostHasContentWithSummary()
        {
            Assert.True(new ExtractedPost { TextSummary = "summary" }.HasContent());
        }

        [Fact]
        public void TestExtractedPostHasContentWithMedia()
        {
            Assert.True(new ExtractedPost { Media = new[] { new Uri("https://example.com") }.ToHashSet() }.HasContent());
        }
    }
}
