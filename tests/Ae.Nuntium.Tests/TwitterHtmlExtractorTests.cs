using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace Ae.Nuntium.Tests;

public class TwitterHtmlExtractorTests
{
    [Fact]
    public async Task ExtractPosts()
    {
        var extractor = new TwitterHtmlExtractor();

        var posts = await extractor.ExtractPosts(new SourceDocument { Body = File.ReadAllText("Files/tweets1.html"), Source = new Uri("https://twitter.com/microsoft", UriKind.Absolute) });

        var json = JsonSerializer.Serialize(posts, new JsonSerializerOptions {  WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        Assert.Equal(File.ReadAllText("Files/tweets1.json"), json);
    }
}