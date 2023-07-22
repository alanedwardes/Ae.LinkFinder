using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class SummaryMarkdownEnricherTests
    {
        [Fact]
        public async Task EnrichExtractedPosts()
        {
            var enricher = new SummaryMarkdownEnricher();

            var post1 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                SummaryContent = "<b>test</b>"
            };

            var post2 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                SummaryContent = "{\"test\":\"wibble\"}"
            };

            var post3 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                SummaryContent = "\t"
            };

            var post4 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                SummaryContent = "<html></body>"
            };

            var post5 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                SummaryContent = "**test**"
            };

            var post6 = new ExtractedPost(new Uri("https://www.example.com/"));

            await enricher.EnrichExtractedPosts(new[] { post1, post2, post3, post4 }, CancellationToken.None);

            Assert.Equal("**test**", post1.SummaryContent);
            Assert.Equal("{\"test\":\"wibble\"}", post2.SummaryContent);
            Assert.Equal("\t", post3.SummaryContent);
            Assert.Equal("<html></body>", post4.SummaryContent);
            Assert.Equal("**test**", post5.SummaryContent);
            Assert.Null(post6.SummaryContent);
        }
    }
}
