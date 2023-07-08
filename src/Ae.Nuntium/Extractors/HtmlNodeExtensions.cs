using Ae.Nuntium.Services;
using HtmlAgilityPack;
using System.Web;

namespace Ae.Nuntium.Extractors
{
    public static class HtmlNodeExtensions
    {
        public static IList<HtmlNode> GetChildrenAndSelf(this HtmlNode node)
        {
            List<HtmlNode> list = new() { node };
            foreach (var child in node.ChildNodes)
            {
                list.AddRange(GetChildrenAndSelf(child));
            }
            return list;
        }

        public static void GetLinksAndMedia(this HtmlNode parent, Uri documentUri, Action<Uri> foundLink, Action<Uri> foundMedia)
        {
            foreach (var node in parent.GetChildrenAndSelf())
            {
                if (node.Name == "a")
                {
                    var href = node.GetAttributeValue<string>("href", null);
                    if (href != "#" && UriExtensions.TryCreateAbsoluteUri(HttpUtility.HtmlDecode(href), documentUri, out var hrefUri))
                    {
                        foundLink(hrefUri);
                    }
                }

                if (node.Name == "img")
                {
                    var src = node.GetAttributeValue<string>("src", null);
                    if (!src.StartsWith("data") && UriExtensions.TryCreateAbsoluteUri(HttpUtility.HtmlDecode(src), documentUri, out var srcUri))
                    {
                        foundMedia(srcUri);
                    }
                }
            }
        }
    }
}
