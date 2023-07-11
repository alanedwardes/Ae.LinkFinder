using Ae.Nuntium.Services;
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
            var posts = new List<ExtractedPost>();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(sourceDocument.Body);

            var feed = htmlDoc.DocumentNode.SelectSingleNode(".//div[@role = 'feed']");

            var articles = feed?.SelectNodes(".//div[@role = 'article'][not(ancestor::div[@role = 'article'])]");

            foreach (var article in articles ?? Enumerable.Empty<HtmlNode>())
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

                article.GetLinksAndMedia(sourceDocument.Source, link => links.Add(link), mediaUri =>
                {
                    if (!mediaUri.PathAndQuery.Contains("rsrc.php") && !mediaUri.PathAndQuery.Contains("emoji"))
                    {
                        media.Add(mediaUri);
                    }
                });

                var permalink = links.FirstOrDefault(x => x.PathAndQuery.Contains("/posts/"));
                if (permalink == null)
                {
                    // If there is no permalink, this is an invalid post
                    continue;
                }

                var extractedPost = new ExtractedPost(permalink.RemoveFragmentAndQueryString())
                {
                    Links = links,
                    Media = media,
                };

                var author = article.SelectSingleNode(".//h2");
                if (author != null)
                {
                    extractedPost.Author = HttpUtility.HtmlDecode(author.InnerText);
                }

                if (article.SelectSingleNode(".//image['@xlink:href' != '']").TryGetUriFromAttribute("xlink:href", sourceDocument.Source, out var avatarUri))
                {
                    extractedPost.Avatar = avatarUri;
                }

                var content = article.SelectSingleNode(".//div[@data-ad-preview = 'message']");
                if (content != null)
                {
                    extractedPost.TextSummary = content.ToMarkdown();
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
