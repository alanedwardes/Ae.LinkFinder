using Ae.Nuntium.Services;
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

            var posts = new List<ExtractedPost>();

            foreach (Match match in regex.Matches(sourceDocument.Body))
            {
                if (UriExtensions.TryCreateAbsoluteUri(match.Value, sourceDocument.Source, out var absoluteUri))
                {
                    posts.Add(new ExtractedPost(absoluteUri));
                }
            }

            return Task.FromResult<IList<ExtractedPost>>(posts);
        }
    }
}
