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

                var toRemove = new HashSet<HtmlNode>();

                foreach (var node in article.GetChildrenAndSelf())
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

                var links = new HashSet<Uri>();
                var media = new HashSet<Uri>();

                foreach (var node in article.GetChildrenAndSelf())
                {
                    if (node.Name == "a")
                    {
                        var href = node.GetAttributeValue<string>("href", null);
                        if (href != "#" && Uri.TryCreate(HttpUtility.HtmlDecode(href), UriKind.Absolute, out var hrefUri))
                        {
                            links.Add(hrefUri);
                        }
                    }

                    if (node.Name == "img")
                    {
                        var src = node.GetAttributeValue<string>("src", null);
                        if (!src.Contains("emoji") && !src.StartsWith("data") && !src.Contains("rsrc.php") && Uri.TryCreate(HttpUtility.HtmlDecode(src), UriKind.Absolute, out var srcUri))
                        {
                            media.Add(srcUri);
                        }
                    }
                }

                var permalink = links.FirstOrDefault(x => x.PathAndQuery.Contains("/posts/"));
                if (permalink == null)
                {
                    // If there is no permalink, this is an invalid post
                    continue;
                }

                // Strip the query string / fragment
                var builder = new UriBuilder(permalink)
                {
                    Query = null,
                    Fragment = null
                };

                var extractedPost = new ExtractedPost(builder.Uri)
                {
                    Links = links,
                    Media = media,
                };

                var author = article.SelectSingleNode(".//h2");
                if (author != null)
                {
                    extractedPost.Author = HttpUtility.HtmlDecode(author.InnerText);
                }

                var content = article.SelectSingleNode(".//div[@data-ad-preview = 'message']");
                if (content != null)
                {
                    extractedPost.TextSummary = HttpUtility.HtmlDecode(content.InnerText);
                    extractedPost.RawContent = content.InnerHtml;
                }

                if (extractedPost.TextSummary == null && !extractedPost.Media.Any())
                {
                    _logger.LogWarning("Unable to find any content for {Permalink}, skipping", extractedPost.Permalink);
                    continue;
                }

                posts.Add(extractedPost);
            }

            return Task.FromResult<IList<ExtractedPost>>(posts);
        }
    }
}
