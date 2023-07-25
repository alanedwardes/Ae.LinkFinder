using Ae.Nuntium.Extractors;
using HtmlAgilityPack;
using System.Web;

namespace Ae.Nuntium.Enrichers
{
    public sealed class HomepageMetadataEnricher : IExtractedPostEnricher
    {
        private sealed class HomepageMetadata
        {
            public string? Title { get; set; }
            public Uri? Icon { get; set; }
        }

        private readonly IHttpClientFactory _factory;
        public HomepageMetadataEnricher(IHttpClientFactory factory) => _factory = factory;

        public async Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            foreach (var domainPosts in posts.GroupBy(x => new Uri(x.Permalink, "/")))
            {
                var info = await GetPageInformation(domainPosts.Key, cancellation);
                if (info == null)
                {
                    continue;
                }

                foreach (var post in domainPosts)
                {
                    post.Author ??= info.Title;
                    post.Avatar ??= info.Icon;
                }
            }
        }

        private async Task<HomepageMetadata> GetPageInformation(Uri pageAddress, CancellationToken cancellation)
        {
            var info = new HomepageMetadata
            {
                // This is the "last ditch" fallback - let google take the wheel (otherwise try to calculate it)
                Icon = new Uri($"https://s2.googleusercontent.com/s2/favicons?domain={pageAddress.Host}&sz=32"),
            };

            using var client = _factory.CreateClient();

            var response = await client.GetAsync(pageAddress, cancellation);
            if (!response.IsSuccessStatusCode)
            {
                return info;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync(cancellation));

            var titleNode = htmlDoc.DocumentNode.SelectSingleNode(".//title");
            if (titleNode != null)
            {
                info.Title = HttpUtility.HtmlDecode(titleNode.InnerText);
            }

            // Prefer the open graph title over the document title
            var openGraphTitleNode = htmlDoc.DocumentNode.SelectSingleNode(".//meta[@property = 'og:title']");
            if (openGraphTitleNode.TryGetTextFromAttribute("content", out var openGraphTitle))
            {
                info.Title = openGraphTitle;
            }

            // And the site name is even better
            var openGraphSiteNameNode = htmlDoc.DocumentNode.SelectSingleNode(".//meta[@property = 'og:site_name']");
            if (openGraphSiteNameNode.TryGetTextFromAttribute("content", out var openGraphSiteName))
            {
                info.Title = openGraphSiteName;
            }

            var openGraphImageNode = htmlDoc.DocumentNode.SelectSingleNode(".//meta[@property = 'og:image']");
            if (openGraphImageNode.TryGetAbsoluteUriFromAttribute("content", out var openGraphImage))
            {
                info.Icon = openGraphImage;
            }

            // Prefer secure image URLs
            var openGraphImageNodeSecure = htmlDoc.DocumentNode.SelectSingleNode(".//meta[@property = 'og:image:secure_url']");
            if (openGraphImageNodeSecure.TryGetAbsoluteUriFromAttribute("content", out var openGraphImageSecure))
            {
                info.Icon = openGraphImageSecure;
            }

            // But prefer the apple touch icon as this is likely most suitable
            var appleTouchIconNode = htmlDoc.DocumentNode.SelectSingleNode(".//link[@rel = 'apple-touch-icon']");
            if (appleTouchIconNode.TryGetUriFromAttribute("href", pageAddress, out var appleTouchIconUri))
            {
                info.Icon = appleTouchIconUri;
            }

            return info;
        }
    }
}
