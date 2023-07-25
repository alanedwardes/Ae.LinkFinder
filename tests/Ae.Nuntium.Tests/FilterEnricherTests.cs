using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class FilterEnricherTests
    {
        [Fact]
        public async Task TestMaxAgeFilter()
        {
            var enricher = new FilterEnricher(NullLogger<FilterEnricher>.Instance, new FilterEnricher.Configuration
            {
                MaxAgeDays = 10
            });

            var post1 = new ExtractedPost(new Uri("https://www.example.com/")) { Published = DateTime.UtcNow };
            var post2 = new ExtractedPost(new Uri("https://www.example.com/")) { Published = DateTime.UtcNow.AddDays(-11) };
            var post3 = new ExtractedPost(new Uri("https://www.example.com/"));

            var list = new List<ExtractedPost> { post1, post2, post3 };

            await enricher.EnrichExtractedPosts(list, CancellationToken.None);

            Assert.Equal(2, list.Count);
            Assert.Same(post1, list[0]);
            Assert.Same(post3, list[1]);
        }
    }
}
