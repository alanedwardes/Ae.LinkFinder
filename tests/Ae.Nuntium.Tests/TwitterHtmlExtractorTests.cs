using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Xunit;

namespace Ae.Nuntium.Tests;

public class TwitterHtmlExtractorTests
{
    [Fact]
    public async Task ExtractTweets()
    {
        var extractor = new TwitterHtmlExtractor();

        var posts = await extractor.ExtractPosts(new SourceDocument { Body = File.ReadAllText("Files/tweets1.html"), Source = new Uri("https://twitter.com/microsoft", UriKind.Absolute) });

        posts.Compare("Files/tweets1.json");
    }

    [Fact]
    public async Task ExtractNoTweets()
    {
        var extractor = new TwitterHtmlExtractor();

        var posts = await extractor.ExtractPosts(new SourceDocument { Body = File.ReadAllText("Files/tweets2.html"), Source = new Uri("https://twitter.com/microsoft", UriKind.Absolute) });

        Assert.Empty(posts);
    }
}