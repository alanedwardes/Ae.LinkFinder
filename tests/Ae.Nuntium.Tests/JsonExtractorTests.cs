using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class JsonExtractorTests
    {
        [Fact]
        public async Task JsonExtractorTest1()
        {
            var extractor = new JsonExtractor(new JsonExtractor.Configuration
            {
                ItemPath = "$.reviews[*]",
                PermalinkFormat = "https://steamcommunity.com/profiles/{author.steamid}/recommended/{appId}",
                TitleFormat = "{voted_up:choose(True|False):Recommended 👍|Not Recommended 👎}",
                TextSummaryFormat = "{review}",
                AuthorFormat = "{author.steamid}",
                AdditionalVariables = new Dictionary<string, string>
                {
                    { "appId", "<appId>" }
                }
            });

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/json1.json"),
                Source = new Uri("https://store.steampowered.com/appreviews/582890?json=1&filter=recent", UriKind.Absolute)
            });

            posts.Compare("Files/json1_posts.json");
        }
    }
}
