using Ae.Nuntium.Sources;
using HtmlAgilityPack;
using System.Web;

namespace Ae.Nuntium.Extractors
{
    public sealed class TwitterHtmlExtractor : IPostExtractor
    {
        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(sourceDocument.Body);

            var tweets = htmlDoc.DocumentNode.SelectNodes(".//article[@data-testid = 'tweet']");

            var extractedPosts = new List<ExtractedPost>();

            foreach (var tweet in tweets)
            {
                var links = new HashSet<Uri>();
                var media = new HashSet<Uri>();
                foreach (var node in tweet.GetChildrenAndSelf())
                {
                    if (node.Name == "a")
                    {
                        var href = node.GetAttributeValue<string>("href", null);
                        if (href != "#" && UriExtensions.TryCreateAbsoluteUri(HttpUtility.HtmlDecode(href), sourceDocument.Source, out var hrefUri))
                        {
                            links.Add(hrefUri);
                        }
                    }

                    if (node.Name == "img")
                    {
                        var src = node.GetAttributeValue<string>("src", null);
                        if (!src.Contains("/profile_images/") && !src.StartsWith("data") && !src.Contains("/hashflags/") && !src.Contains("/emoji/") && UriExtensions.TryCreateAbsoluteUri(HttpUtility.HtmlDecode(src), sourceDocument.Source, out var srcUri))
                        {
                            media.Add(srcUri);
                        }
                    }
                }

                var permalinks = links.Where(x => sourceDocument.Source.IsBaseOf(x) && x.PathAndQuery.Contains("/status/") && !x.PathAndQuery.EndsWith("/analytics"));
                if (!permalinks.Any())
                {
                    // This is invalid if there are no appropriate permalinks
                    continue;
                }

                var extractedPost = new ExtractedPost(permalinks.First())
                {
                    Links = links,
                    Media = media,
                };

                var content = tweet.SelectSingleNode(".//div[@data-testid = 'tweetText']");
                if (content != null)
                {
                    extractedPost.TextSummary = content.InnerText;
                    extractedPost.RawContent = content.InnerHtml;
                }

                var author = tweet.SelectSingleNode(".//div[@data-testid = 'User-Name']");
                if (author != null)
                {
                    var spans = author.SelectNodes(".//span");
                    if (spans.Any())
                    {
                        extractedPost.Author = spans.First().InnerText;
                    }
                }

                var time = tweet.SelectSingleNode(".//time");
                if (time != null)
                {
                    var datetime = time.GetAttributeValue<string>("datetime", null);
                    if (datetime != null && DateTimeOffset.TryParse(datetime, out var resultingDateTime))
                    {
                        extractedPost.Published = resultingDateTime.UtcDateTime;
                    }
                }

                extractedPosts.Add(extractedPost);
            }

            return Task.FromResult<IList<ExtractedPost>>(extractedPosts);
        }
    }
}
