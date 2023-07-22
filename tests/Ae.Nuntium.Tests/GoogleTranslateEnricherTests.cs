using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Google;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class GoogleTranslateEnricherTests : IDisposable
    {
        private readonly MockRepository _repository = new(MockBehavior.Strict);

        public void Dispose() => _repository.VerifyAll();

        [Fact]
        public async Task GoogleTranslateEnricher()
        {
            var translationClient = _repository.Create<TranslationClient>();
            var configuration = new GoogleTranslateEnricher.Configuration();

            var enricher = new GoogleTranslateEnricher(NullLogger<GoogleTranslateEnricher>.Instance, translationClient.Object, configuration);

            var post1 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                TextSummary = "translate me"
            };

            var post2 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                TextSummary = "translation error"
            };

            var post3 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                TextSummary = "untranslatable"
            };

            var post4 = new ExtractedPost(new Uri("https://www.example.com/"))
            {
                TextSummary = "null result"
            };

            translationClient.Setup(x => x.TranslateTextAsync("translate me", "en", null, null, CancellationToken.None))
                .ReturnsAsync(new TranslationResult("translate me", "translated", "fr", null, "en", null));

            translationClient.Setup(x => x.TranslateTextAsync("translation error", "en", null, null, CancellationToken.None))
                .ThrowsAsync(new GoogleApiException("serviceName"));

            translationClient.Setup(x => x.TranslateTextAsync("untranslatable", "en", null, null, CancellationToken.None))
                .ReturnsAsync(new TranslationResult("translate me", "translated", "en", null, "en", null));

            translationClient.Setup(x => x.TranslateTextAsync("null result", "en", null, null, CancellationToken.None))
                .ReturnsAsync((TranslationResult?)null);

            await enricher.EnrichExtractedPosts(new[] { post1, post2, post3, post4 }, CancellationToken.None);

            Assert.Equal("(Translated from FR) translated", post1.TextSummary);
            Assert.Equal("translation error", post2.TextSummary);
            Assert.Equal("untranslatable", post3.TextSummary);
            Assert.Equal("null result", post4.TextSummary);
        }
    }
}
