using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Xunit;

namespace Ae.Nuntium.Tests;

public class TwitterHtmlExtractorTests
{
    [Fact]
    public async Task ExtractPosts()
    {
        var extractor = new TwitterHtmlExtractor();

        var posts = await extractor.ExtractPosts(new SourceDocument { Body = File.ReadAllText("Files/tweets1.html"), Source = new Uri("https://twitter.com/microsoft", UriKind.Absolute) });

        Assert.Equal(File.ReadAllText("Files/tweets1.json"), posts.ToJson());
    }
}