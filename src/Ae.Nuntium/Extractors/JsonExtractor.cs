using Ae.Nuntium.Sources;
using Microsoft.Extensions.Logging;
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
            public string ContentFormat { get; set; }
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
                .AddExtensions(new ListFormatter(), new NullFormatter(), new DefaultFormatter());
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
                
                string? content = null;
                if (_configuration.ContentFormat != null)
                {
                    content = formatter.Format(_configuration.ContentFormat, token);
                }

                string? author = null;
                if (_configuration.AuthorFormat != null)
                {
                    author = formatter.Format(_configuration.AuthorFormat, token);
                }

                extractedPosts.Add(new ExtractedPost
                {
                    Permalink = permalink == null ? null : new Uri(permalink, UriKind.Absolute),
                    Content = content,
                    Author = author,
                });
            }

            return Task.FromResult<IList<ExtractedPost>>(extractedPosts);
        }
    }
}