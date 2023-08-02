using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class HtmlEditorEnricherTests
    {
        [Fact]
        public async Task TestStripImages()
        {
            var post = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                Body = "<img src=\"test\"/>wibble",
                Summary = "<img src=\"test\"/>wibble"
            };

            var enricher = new HtmlEditorEnricher(new HtmlEditorEnricher.Configuration { StripImages = true });

            await enricher.EnrichExtractedPosts(new[] { post }, CancellationToken.None);

            Assert.Equal("wibble", post.Body);
            Assert.Equal("wibble", post.Summary);
        }

        [Fact]
        public async Task TestKeepFirstParagraph()
        {
            var post = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                Body = "<p>test1</p><p>test2</p>",
                Summary = "<p>test1</p><p>test2</p>"
            };

            var enricher = new HtmlEditorEnricher(new HtmlEditorEnricher.Configuration { KeepFirstParagraph = true });

            await enricher.EnrichExtractedPosts(new[] { post }, CancellationToken.None);

            Assert.Equal("<p>test1</p>", post.Body);
            Assert.Equal("<p>test1</p>", post.Summary);
        }
    }
}
