using Ae.Nuntium.Sources;
using Amazon.Runtime.Internal.Util;
using HtmlAgilityPack;
using SmartReader;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

namespace Ae.Nuntium.Extractors
{
    public sealed class RssExtractor : IPostExtractor
    {
        public static HtmlDocument? TryParseHtml(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var htmlDoc = new HtmlDocument();
            try
            {
                htmlDoc.LoadHtml(content);
            }
            catch
            {
                return null;
            }

            if (htmlDoc.DocumentNode.ChildNodes.Count == 1 && htmlDoc.DocumentNode.ChildNodes[0].NodeType == HtmlNodeType.Text)
            {
                return null;
            }

            return htmlDoc;
        }

        public string FixupAuthor(string rssXml)
        {
            if (!rssXml.Contains("<author"))
            {
                return rssXml;
            }

            XmlDocument xmldoc = new();
            xmldoc.LoadXml(rssXml);

            foreach (XmlElement author in xmldoc.GetElementsByTagName("author"))
            {
                if (author.ChildNodes.Count > 1)
                {
                    foreach (XmlElement name in author.GetElementsByTagName("name"))
                    {
                        author.InnerText = name.InnerText;
                        break;
                    }
                }
            }

            return xmldoc.OuterXml;
        }

        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var feedXml = FixupAuthor(sourceDocument.Body);

            using var sr = new StringReader(feedXml);

            SyndicationFeed feed;
            using (XmlReader reader = XmlReader.Create(sr))
            {
                feed = SyndicationFeed.Load(reader);
            }

            var extractedPosts = new List<ExtractedPost>();

            foreach (SyndicationItem item in feed.Items)
            {
                var permalink = item.Links.FirstOrDefault()?.Uri;
                var content = item.ElementExtensions.ReadElementExtensions<string>("encoded", "http://purl.org/rss/1.0/modules/content/").FirstOrDefault() ?? (item.Content as TextSyndicationContent)?.Text;
                var author = item.ElementExtensions.ReadElementExtensions<string>("creator", "http://purl.org/dc/elements/1.1/").FirstOrDefault() ?? item.Authors.FirstOrDefault()?.Name ?? item.Authors.FirstOrDefault()?.Email;

                var extractedPost = new ExtractedPost(permalink)
                {
                    Title = item.Title?.Text?.Trim(),
                    Author = author?.Trim(),
                    Published = item.PublishDate.UtcDateTime
                };

                var summaryHtml = TryParseHtml(item.Summary?.Text);
                if (summaryHtml == null)
                {
                    extractedPost.TextSummary = item.Summary?.Text?.ToMarkdown();
                }
                else
                {
                    extractedPost.TextSummary = summaryHtml.DocumentNode.ToMarkdown();
                }

                var contentHtml = TryParseHtml(content);
                if (contentHtml != null)
                {
                    extractedPost.RawContent = contentHtml.DocumentNode.InnerHtml?.Trim();
                }
                else
                {
                    extractedPost.RawContent = (content ?? summaryHtml?.DocumentNode.InnerHtml)?.Trim();
                }

                var article = contentHtml?.DocumentNode ?? summaryHtml?.DocumentNode ?? new HtmlDocument().DocumentNode;

                article.GetLinksAndMedia(sourceDocument.Source, link => extractedPost.Links.Add(link), mediaUri => extractedPost.Media.Add(mediaUri));

                extractedPosts.Add(extractedPost);
            }

            return Task.FromResult<IList<ExtractedPost>>(extractedPosts);
        }
    }
}
