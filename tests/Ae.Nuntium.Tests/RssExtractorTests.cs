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

            posts.Compare("Files/feed1.json");
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

            posts.Compare("Files/feed2.json");
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

            posts.Compare("Files/feed3.json");
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

            posts.Compare("Files/feed4.json");
        }

        [Fact]
        public async Task RssExtractorFeed5()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed5.xml"),
                Source = new Uri("https://blog.google/rss/", UriKind.Absolute)
            });

            posts.Compare("Files/feed5.json");
        }

        [Fact]
        public async Task RssExtractorFeed6()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed6.xml"),
                Source = new Uri("https://blog.google/rss/", UriKind.Absolute)
            });

            posts.Compare("Files/feed6.json");
        }

        [Fact]
        public async Task RssExtractorFeed7()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed7.xml"),
                Source = new Uri("https://blog.google/rss/", UriKind.Absolute)
            });

            posts.Compare("Files/feed7.json");
        }

        [Fact]
        public async Task RssExtractorFeed8()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed8.xml"),
                Source = new Uri("https://www.nasa.gov/rss/dyn/lg_image_of_the_day.rss", UriKind.Absolute)
            });

            posts.Compare("Files/feed8.json");
        }

        [Fact]
        public async Task RssExtractorFeed9()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed9.xml"),
                Source = new Uri("http://esawebb.org/images/feed/", UriKind.Absolute)
            });

            posts.Compare("Files/feed9.json");
        }

        [Fact]
        public async Task RssExtractorFeed10()
        {
            var extractor = new RssExtractor();

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/feed10.xml"),
                Source = new Uri("http://esahubble.org/images/feed/", UriKind.Absolute)
            });

            posts.Compare("Files/feed10.json");
        }
    }
}
