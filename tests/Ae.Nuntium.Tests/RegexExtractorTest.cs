using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class RegexExtractorTest
    {
        [Fact]
        public async Task RegexExtractor1()
        {
            var extractor = new RegexExtractor(new RegexExtractor.Configuration
            {
                Pattern = @"https://steamcommunity.com/sharedfiles/filedetails/\?id=([0-9]*)"
            });

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/regex1.html"),
                Source = new Uri("https://steamcommunity.com/app/582890/screenshots/?browsefilter=mostrecent", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/regex1.json"), posts.ToJson());
        }

        [Fact]
        public async Task RegexExtractor2()
        {
            var extractor = new RegexExtractor(new RegexExtractor.Configuration
            {
                Pattern = @"https://steamcommunity.com/app/([0-9]*)/discussions/([0-9]*)/([0-9]*)/"
            });

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/regex2.html"),
                Source = new Uri("https://steamcommunity.com/app/582890/discussions/", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/regex2.json"), posts.ToJson());
        }
    }
}
