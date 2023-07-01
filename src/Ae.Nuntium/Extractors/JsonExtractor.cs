using Ae.Nuntium.Sources;
using Newtonsoft.Json.Linq;
using SmartFormat;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace Ae.Nuntium.Extractors
{
    public sealed class JsonExtractor : IPostExtractor
    {
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public string ItemPath { get; set; }
            public string PermalinkFormat { get; set; }
            public string TitleFormat { get; set; }
            public string RawContentFormat { get; set; }
            public string TextSummaryFormat { get; set; }
            public string AuthorFormat { get; set; }
        }

        public JsonExtractor(Configuration configuration)
        {
            _configuration = configuration;
        }

        private static SmartFormatter GetFormatterWithJsonSource(SmartSettings? settings = null)
        {
            var smart = new SmartFormatter(settings ?? new SmartSettings())
                // NewtonsoftJsonSource MUST be registered before ReflectionSource (which is not required here)
                // We also need the ListFormatter to process arrays
                .AddExtensions(new ListFormatter(), new NewtonsoftJsonSource(), new DefaultSource())
                .AddExtensions(new NullFormatter(), new DefaultFormatter(), new ChooseFormatter());
            return smart;
        }

        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var formatter = GetFormatterWithJsonSource();

            var extractedPosts = new List<ExtractedPost>();

            foreach (var token in JObject.Parse(sourceDocument.Body).SelectTokens(_configuration.ItemPath))
            {
                string? permalink = null;
                if (_configuration.PermalinkFormat != null)
                {
                    permalink = formatter.Format(_configuration.PermalinkFormat, token);
                }
                
                string? rawContent = null;
                if (_configuration.RawContentFormat != null)
                {
                    rawContent = formatter.Format(_configuration.RawContentFormat, token);
                }

                string? author = null;
                if (_configuration.AuthorFormat != null)
                {
                    author = formatter.Format(_configuration.AuthorFormat, token);
                }

                string? textSummary = null;
                if (_configuration.TitleFormat != null)
                {
                    textSummary = formatter.Format(_configuration.TextSummaryFormat, token);
                }

                string? title = null;
                if (_configuration.TitleFormat != null)
                {
                    title = formatter.Format(_configuration.TitleFormat, token);
                }

                extractedPosts.Add(new ExtractedPost
                {
                    Permalink = permalink == null ? null : new Uri(permalink, UriKind.Absolute),
                    RawContent = rawContent,
                    TextSummary = textSummary,
                    Title = title,
                    Author = author,
                });
            }

            return Task.FromResult<IList<ExtractedPost>>(extractedPosts);
        }
    }
}