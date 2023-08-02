using Ae.Nuntium.Sources;
using HtmlAgilityPack;
using System.ServiceModel.Syndication;
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
                if (summaryHtml != null)
                {
                    summaryHtml.DocumentNode.MakeRelativeUrisAbsolute(sourceDocument.Address);
                    extractedPost.TextSummary = summaryHtml.DocumentNode.InnerHtml;
                }
                else
                {
                    extractedPost.TextSummary = item.Summary?.Text;
                }

                var contentHtml = TryParseHtml(content);
                if (contentHtml != null)
                {
                    contentHtml.DocumentNode.MakeRelativeUrisAbsolute(sourceDocument.Address);
                    extractedPost.RawContent = contentHtml.DocumentNode.InnerHtml?.Trim();
                }
                else
                {
                    extractedPost.RawContent = (content ?? summaryHtml?.DocumentNode.InnerHtml)?.Trim();
                }

                var article = contentHtml?.DocumentNode ?? summaryHtml?.DocumentNode ?? new HtmlDocument().DocumentNode;

                article.GetLinksAndMedia(sourceDocument.Address, link => extractedPost.Links.Add(link), mediaUri => extractedPost.Media.Add(mediaUri));

                foreach (var link in item.Links ?? Enumerable.Empty<SyndicationLink>())
                {
                    if (link.RelationshipType == "enclosure" && link.MediaType != null && link.MediaType.StartsWith("image"))
                    {
                        extractedPost.Thumbnail = link.GetAbsoluteUri();
                    }
                    else if (link.MediaType != null && (link.MediaType.StartsWith("image") || link.MediaType.StartsWith("video")))
                    {
                        extractedPost.Media.Add(link.GetAbsoluteUri());
                    }
                    else if (extractedPost.Permalink != link.GetAbsoluteUri())
                    {
                        extractedPost.Links.Add(link.GetAbsoluteUri());
                    }
                }

                extractedPosts.Add(extractedPost);
            }

            return Task.FromResult<IList<ExtractedPost>>(extractedPosts);
        }
    }
}
