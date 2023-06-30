using Ae.Nuntium.Extractors;
using Ae.Nuntium.Sources;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ae.Nuntium.Tests;

public class FacebookGroupHtmlExtractorTests
{
    [Fact]
    public async Task ExtractPosts()
    {
        var extractor = new FacebookGroupHtmlExtractor(NullLogger<FacebookGroupHtmlExtractor>.Instance);

        var posts = await extractor.ExtractPosts(new SourceDocument { Body = File.ReadAllText("Files/group1.html") });

        posts.Compare("Files/group1.json");
    }
}