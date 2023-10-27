using Ae.Nuntium.Extractors;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium.Enrichers
{
    public sealed class HtmlEditorEnricher : IExtractedPostEnricher
    {
        private readonly ILogger<HtmlEditorEnricher> _logger;
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public bool StripImages { get; set; }
            public bool KeepFirstParagraph { get; set; }
        }

        public HtmlEditorEnricher(ILogger<HtmlEditorEnricher> logger, Configuration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            foreach (var post in posts)
            {
                if (post.Summary != null)
                {
                    post.Summary = EditHtml(post.Summary);
                }

                if (post.Body != null)
                {
                    post.Body = EditHtml(post.Body);
                }
            }

            return Task.CompletedTask;
        }

        public string EditHtml(string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);

            if (_configuration.StripImages)
            {
                var images = html.DocumentNode.SelectNodes(".//img");
                if (images != null)
                {
                    RemoveChildren(html.DocumentNode, images);
                }
            }

            if (_configuration.KeepFirstParagraph)
            {
                var paragraphs = html.DocumentNode.SelectNodes(".//p[position() > 1]");
                if (paragraphs != null)
                {
                    RemoveChildren(html.DocumentNode, paragraphs);
                }
            }

            return html.DocumentNode.OuterHtml;
        }

        public void RemoveChildren(HtmlNode parent, HtmlNodeCollection children)
        {
            try
            {
                parent.RemoveChildren(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to remove child nodes");
            }
        }
    }
}
