using Ae.Nuntium.Sources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
                .AddExtensions(new NullFormatter(), new DefaultFormatter(), new ChooseFormatter(), new IsMatchFormatter());
            return smart;
        }

        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var formatter = GetFormatterWithJsonSource();

            var extractedPosts = new List<ExtractedPost>();

            var sourceToken = JToken.FromObject(sourceDocument, new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            foreach (var itemToken in JObject.Parse(sourceDocument.Body).SelectTokens(_configuration.ItemPath))
            {
                var parameters = new JObject
                {
                    ["item"] = itemToken,
                    ["source"] = sourceToken
                };

                string? permalink = null;
                if (_configuration.PermalinkFormat != null)
                {
                    permalink = formatter.Format(_configuration.PermalinkFormat, parameters);
                }
                
                string? rawContent = null;
                if (_configuration.RawContentFormat != null)
                {
                    rawContent = formatter.Format(_configuration.RawContentFormat, parameters);
                }

                string? author = null;
                if (_configuration.AuthorFormat != null)
                {
                    author = formatter.Format(_configuration.AuthorFormat, parameters);
                }

                string? textSummary = null;
                if (_configuration.TitleFormat != null)
                {
                    textSummary = formatter.Format(_configuration.TextSummaryFormat, parameters);
                }

                string? title = null;
                if (_configuration.TitleFormat != null)
                {
                    title = formatter.Format(_configuration.TitleFormat, parameters);
                }

                extractedPosts.Add(new ExtractedPost(new Uri(permalink, UriKind.Absolute))
                {
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