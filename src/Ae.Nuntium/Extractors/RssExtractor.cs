using Ae.Nuntium.Sources;
using HtmlAgilityPack;
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

            return htmlDoc;
        }

        public string FixupAuthor(string rssXml)
        {
            if (!rssXml.Contains("<author"))
            {
                return rssXml;
            }

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(rssXml);

            foreach (XmlElement author in xmldoc.GetElementsByTagName("author"))
            {
                foreach (XmlElement name in author.GetElementsByTagName("name"))
                {
                    author.InnerText = name.InnerText;
                    break;
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
                var content = item.ElementExtensions.ReadElementExtensions<string>("encoded", "http://purl.org/rss/1.0/modules/content/").FirstOrDefault();
                var author = item.ElementExtensions.ReadElementExtensions<string>("creator", "http://purl.org/dc/elements/1.1/").FirstOrDefault() ?? item.Authors.FirstOrDefault()?.Name ?? item.Authors.FirstOrDefault()?.Email;

                var extractedPost = new ExtractedPost
                {
                    Title = item.Title?.Text,
                    Permalink = permalink,
                    Content = content,
                    Author = author
                };

                HtmlDocument? html;
                if (content == null)
                {
                    extractedPost.Content = item.Summary?.Text;
                    html = TryParseHtml(item.Summary?.Text);
                }
                else
                {
                    extractedPost.Summary = item.Summary?.Text;
                    extractedPost.Content = content;
                    html = TryParseHtml(content);
                }

                foreach (var node in GetChildrenAndSelf(html.DocumentNode))
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
                        if (!src.StartsWith("data") && Uri.TryCreate(HttpUtility.HtmlDecode(src), UriKind.Absolute, out var srcUri))
                        {
                            extractedPost.Media.Add(srcUri);
                        }
                    }
                }

                extractedPosts.Add(extractedPost);
            }

            return Task.FromResult<IList<ExtractedPost>>(extractedPosts);
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
