using Ae.Nuntium.Services;
using Ae.Nuntium.Sources;
using HtmlAgilityPack;

namespace Ae.Nuntium.Extractors
{
    public sealed class TwitterHtmlExtractor : IPostExtractor
    {
        public Task<IList<ExtractedPost>> ExtractPosts(SourceDocument sourceDocument)
        {
            var extractedPosts = new List<ExtractedPost>();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(sourceDocument.Body);

            var tweets = htmlDoc.DocumentNode.SelectNodes(".//article[@data-testid = 'tweet']");

            foreach (var tweet in tweets ?? Enumerable.Empty<HtmlNode>())
            {
                var links = new HashSet<Uri>();
                var media = new HashSet<Uri>();

                tweet.GetLinksAndMedia(sourceDocument.Source, link => links.Add(link), mediaUri =>
                {
                    if (!mediaUri.PathAndQuery.Contains("/profile_images/", StringComparison.InvariantCultureIgnoreCase) &&
                        !mediaUri.PathAndQuery.Contains("/hashflags/", StringComparison.InvariantCultureIgnoreCase) &&
                        !mediaUri.PathAndQuery.Contains("/emoji/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        media.Add(mediaUri);
                    }
                });

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

                var avatar = tweet.SelectSingleNode(".//div[@data-testid = 'Tweet-User-Avatar']");
                if (avatar != null)
                {
                    var img = avatar.SelectSingleNode(".//img");
                    if (img != null && UriExtensions.TryCreateAbsoluteUri(img.GetAttributeValue("src", null), sourceDocument.Source, out var avatarUri))
                    {
                        extractedPost.Avatar = avatarUri;
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
