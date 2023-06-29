using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class RssExtractorTests
    {
        [Fact]
        public async Task RssExtractorFeed1()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed1.xml"),
                Source = new Uri("https://github.blog/feed/", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/feed1.json"), posts.ToJson());
        }

        [Fact]
        public async Task RssExtractorFeed2()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed2.xml"),
                Source = new Uri("https://devblogs.microsoft.com/dotnet/feed/", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/feed2.json"), posts.ToJson());
        }

        [Fact]
        public async Task RssExtractorFeed3()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed3.xml"),
                Source = new Uri("https://blog.google/rss/", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/feed3.json"), posts.ToJson());
        }

        [Fact]
        public async Task RssExtractorFeed4()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed4.xml"),
                Source = new Uri("https://blog.google/rss/", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/feed4.json"), posts.ToJson());
        }
    }
}
