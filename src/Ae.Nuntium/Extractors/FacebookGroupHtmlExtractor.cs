﻿using Ae.Nuntium.Services;
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
                article.MakeRelativeUrisAbsolute(sourceDocument.Address);

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

                // Replace all emoji images with a span element + the emoji characters
                foreach (var node in article.GetChildrenAndSelf())
                {
                    if (node.Name == "img")
                    {
                        var src = node.GetAttributeValue("src", string.Empty);
                        if (!src.Contains("emoji.php"))
                        {
                            continue;
                        }

                        var alt = node.GetAttributeValue("alt", string.Empty);
                        if (!string.IsNullOrEmpty(alt))
                        {
                            node.Name = "span";
                            node.InnerHtml = alt;
                            node.Attributes.RemoveAll();
                        }
                    }
                }

                var links = new HashSet<Uri>();
                var media = new HashSet<Uri>();

                article.GetLinksAndMedia(sourceDocument.Address, link => links.Add(link), mediaUri =>
                {
                    if (!mediaUri.PathAndQuery.Contains("rsrc.php"))
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

                if (article.SelectSingleNode(".//image['@xlink:href' != '']").TryGetAbsoluteUriFromAttribute("xlink:href", out var avatarUri))
                {
                    extractedPost.Avatar = avatarUri;
                }

                var content = article.SelectSingleNode(".//div[@data-ad-preview = 'message']");
                if (content != null)
                {
                    extractedPost.RawContent = content.InnerHtml;
                }

                if (extractedPost.RawContent == null && !extractedPost.Media.Any())
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
