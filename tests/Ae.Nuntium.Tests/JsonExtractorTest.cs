﻿using Ae.Nuntium.Extractors;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class JsonExtractorTest
    {
        [Fact]
        public async Task JsonExtractorTest1()
        {
            var extractor = new JsonExtractor(new JsonExtractor.Configuration
            {
                ItemPath = "$.reviews[*]",
                PermalinkFormat = "https://steamcommunity.com/profiles/{author.steamid}/recommended/582890",
                ContentFormat = "Positive: {voted_up} Purchased: {steam_purchase}\n{review}",
                AuthorFormat = "{author.steamid}"
            });

            var posts = await extractor.ExtractPosts(new Sources.SourceDocument
            {
                Body = File.ReadAllText("Files/json1.json"),
                Source = new Uri("https://store.steampowered.com/appreviews/582890?json=1&filter=recent", UriKind.Absolute)
            });

            Assert.Equal(File.ReadAllText("Files/json1_posts.json"), posts.ToJson());
        }
    }
}
