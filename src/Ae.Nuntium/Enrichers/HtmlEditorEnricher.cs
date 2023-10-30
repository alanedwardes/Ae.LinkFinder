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
                try
                {
                    ProcessPost(post);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to enrich {Post}", post);
                }
            }

            return Task.CompletedTask;
        }

        private void ProcessPost(ExtractedPost post)
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

        public string EditHtml(string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);

            if (_configuration.StripImages)
            {
                RemoveChildren(html.DocumentNode, ".//img");
            }
            if (_configuration.KeepFirstParagraph)
            {
                RemoveChildren(html.DocumentNode, ".//p[position() > 1]");
            }

            return html.DocumentNode.OuterHtml;
        }

        public void RemoveChildren(HtmlNode parent, string selector)
        {
            HtmlNode node;
            do
            {
                node = parent.SelectSingleNode(selector);
                node?.ParentNode.RemoveChild(node);
            }
            while (node != null);
        }
    }
}
