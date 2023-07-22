using Ae.Nuntium.Extractors;
using Google;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium.Enrichers
{
    public sealed class GoogleTranslateEnricher : IExtractedPostEnricher
    {
        public sealed class Configuration
        {
            public string? SourceLanguage { get; set; }
            public string TargetLanguage { get; set; } = "en";
        }

        private readonly ILogger<GoogleTranslateEnricher> _logger;
        private readonly TranslationClient _translation;
        private readonly Configuration _configuration;

        public GoogleTranslateEnricher(ILogger<GoogleTranslateEnricher> logger, TranslationClient translation, Configuration configuration)
        {
            _logger = logger;
            _translation = translation;
            _configuration = configuration;
        }

        public async Task EnrichExtractedPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            await Task.WhenAll(posts.Select(x => TranslatePost(x, cancellation)));
        }

        public async Task TranslatePost(ExtractedPost post, CancellationToken cancellation)
        {
            if (post.TextSummary == null)
            {
                return;
            }

            TranslationResult result;
            try
            {
                result = await _translation.TranslateTextAsync(post.TextSummary, _configuration.TargetLanguage, _configuration.SourceLanguage, cancellationToken: cancellation);
            }
            catch (GoogleApiException ex)
            {
                _logger.LogError(ex, "Encountered error translating review.");
                return;
            }

            if (result == null)
            {
                _logger.LogWarning("Translation result was null");
                return;
            }

            if (result.DetectedSourceLanguage == _configuration.TargetLanguage)
            {
                _logger.LogInformation("Target language is the same as the language which was detected ({Language})", result.DetectedSourceLanguage);
                return;
            }

            var sourceLanguage = result.DetectedSourceLanguage.ToUpperInvariant();
            post.TextSummary = $"(Translated from {sourceLanguage}) {result.TranslatedText}";
        }
    }
}
