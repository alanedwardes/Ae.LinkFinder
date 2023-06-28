using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace Ae.Nuntium.Tests;

public class FacebookGroupHtmlExtractorTests
{
    [Fact]
    public async Task ExtractPosts()
    {
        var extractor = new FacebookGroupHtmlExtractor(NullLogger<FacebookGroupHtmlExtractor>.Instance);

        var posts = await extractor.ExtractPosts(new SourceDocument { Body = File.ReadAllText("Files/group1.html") });

        var json = JsonSerializer.Serialize(posts, new JsonSerializerOptions {  WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        Assert.Equal(File.ReadAllText("Files/group1.json"), json);
    }
}