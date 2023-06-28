using Ae.Nuntium.Sources;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Ae.Nuntium.Extractors
{
    public sealed class FacebookGroupHtmlExtractor : IPostExtractor
    {
        private readonly ILogger<FacebookGroupHtmlExtractor> _logger;

        public FacebookGroupHtmlExtractor(ILogger<FacebookGroupHtmlExtractor> logger)
        {
            _logger = logger;
        }

        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(sourceDocument.Body);

            var feed = htmlDoc.DocumentNode.SelectSingleNode(".//div[@role = 'feed']");

            var articles = feed.SelectNodes(".//div[@role = 'article'][not(ancestor::div[@role = 'article'])]");

            var posts = new List<ExtractedPost>();

            foreach (var article in articles)
            {
                if (!article.ChildNodes.Any())
                {
                    continue;
                }

                // If it was loading at the time of capture
                if (article.ChildNodes.First().GetAttributeValue<string>("role", null) == "progressbar")
                {
                    continue;
                }

                var extractedPost = new ExtractedPost
                {
                    Links = new HashSet<Uri>(),
                    Media = new HashSet<Uri>()
                };

                var toRemove = new HashSet<HtmlNode>();

                foreach (var node in GetChildrenAndSelf(article))
                {
                    // Remove comments
                    if (node != article && node.GetAttributeValue<string>("role", null) == "article")
                    {
                        toRemove.Add(node);
                    }
                }

                foreach (var node in toRemove)
                {
                    node.ParentNode.RemoveChild(node);
                }

                foreach (var node in GetChildrenAndSelf(article))
                {
                    if (node.Name == "a")
                    {
                        var href = node.GetAttributeValue<string>("href", null);
                        if (href != "#" && Uri.TryCreate(HttpUtility.HtmlDecode(href), UriKind.Absolute, out var hrefUri))
                        {
                            extractedPost.Links.Add(hrefUri);
                        }
                    }

                    if (node.Name == "img")
                    {
                        var src = node.GetAttributeValue<string>("src", null);
                        if (!src.Contains("emoji") && !src.StartsWith("data") && Uri.TryCreate(HttpUtility.HtmlDecode(src), UriKind.Absolute, out var srcUri))
                        {
                            extractedPost.Media.Add(srcUri);
                        }
                    }
                }

                var author = article.SelectSingleNode(".//h2");
                if (author != null)
                {
                    extractedPost.Author = HttpUtility.HtmlDecode(author.InnerText);
                }

                var content = article.SelectSingleNode(".//div[@data-ad-preview = 'message']");
                if (content != null)
                {
                    extractedPost.Content = HttpUtility.HtmlDecode(content.InnerText);
                }

                var permalink = extractedPost.Links.FirstOrDefault(x => x.PathAndQuery.Contains("/posts/"));
                if (permalink != null)
                {
                    // Strip the query string / fragment
                    var builder = new UriBuilder(permalink)
                    {
                        Query = null,
                        Fragment = null
                    };
                    extractedPost.Permalink = builder.Uri;
                }

                if (extractedPost.Content == null && !extractedPost.Media.Any())
                {
                    _logger.LogWarning("Unable to find any content for {Permalink}, skipping", extractedPost.Permalink);
                    continue;
                }

                posts.Add(extractedPost);
            }

            return Task.FromResult<IList<ExtractedPost>>(posts);
        }

        private IList<HtmlNode> GetChildrenAndSelf(HtmlNode node)
        {
            List<HtmlNode> list = new() { node };
            foreach (var child in node.ChildNodes)
            {
                list.AddRange(GetChildrenAndSelf(child));
            }
            return list;
        }
    }
}
