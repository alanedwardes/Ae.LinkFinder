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
                    if (node.TryGetUriFromAttribute("href", documentUri, out var hrefUri))
                    {
                        foundLink(hrefUri);
                    }
                }

                if (node.Name == "img")
                {
                    if (node.TryGetUriFromAttribute("src", documentUri, out var srcUri))
                    {
                        foundMedia(srcUri);
                    }
                }
            }
        }

        public static bool TryGetUriFromAttribute(this HtmlNode node, string attributeName, Uri baseAddress, out Uri newUri)
        {
            if (node == null)
            {
                newUri = null;
                return false;
            }

            var attributeValue = node.GetAttributeValue<string>(attributeName, null);
            if (attributeValue == null)
            {
                newUri = null;
                return false;
            }

            return UriExtensions.TryCreateAbsoluteUri(HttpUtility.HtmlDecode(attributeValue.Trim()), baseAddress, out newUri);
        }

        public static string ToMarkdown(this HtmlNode node)
        {
            return node.InnerHtml.ToMarkdown();
        }

        public static string ToMarkdown(this string html)
        {
            var converter = new ReverseMarkdown.Converter();
            return converter.Convert(html);
        }
    }
}
