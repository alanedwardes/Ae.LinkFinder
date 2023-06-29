using Ae.Nuntium.Sources;
using System.Text.RegularExpressions;

namespace Ae.Nuntium.Extractors
{
    public sealed class RegexExtractor : IPostExtractor
    {
        public sealed class Configuration
        {
            public string Pattern { get; set; }
        }

        private readonly Configuration _configuration;

        public RegexExtractor(Configuration configuration)
        {
            _configuration = configuration;
        }

        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var regex = new Regex(_configuration.Pattern);

            return Task.FromResult<IList<ExtractedPost>>(regex
                .Matches(sourceDocument.Body)
                .Select(x => new ExtractedPost
                {
                    Permalink = new Uri(x.Value, UriKind.Absolute)
                })
                .ToList());
        }
    }
}
