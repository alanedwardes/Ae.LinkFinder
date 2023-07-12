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
                    if (node.TryGetAbsoluteUriFromAttribute("href", out var hrefUri))
                    {
                        foundLink(hrefUri);
                    }
                }

                if (node.Name == "img")
                {
                    if (node.TryGetAbsoluteUriFromAttribute("src", out var srcUri))
                    {
                        foundMedia(srcUri);
                    }
                }
            }
        }

        public static bool TryGetAbsoluteUriFromAttribute(this HtmlNode node, string attributeName, out Uri newUri)
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

            return Uri.TryCreate(HttpUtility.HtmlDecode(attributeValue.Trim()), UriKind.Absolute, out newUri);
        }

        private static bool TryGetUriFromAttribute(this HtmlNode node, string attributeName, Uri baseAddress, out Uri newUri)
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

            return UriExtensions.TryCreateAbsoluteUri(attributeValue.Trim(), baseAddress, out newUri);
        }

        public static void MakeRelativeUrisAbsolute(this HtmlNode parent, Uri documentUri)
        {
            foreach (var node in parent.GetChildrenAndSelf())
            {
                if (node.Name == "a")
                {
                    if (node.TryGetUriFromAttribute("href", documentUri, out var hrefUri))
                    {
                        node.SetAttributeValue("href", hrefUri.AbsoluteUri);
                    }
                    else
                    {
                        node.Attributes.Remove("href");
                    }
                }

                if (node.Name == "img")
                {
                    if (node.TryGetUriFromAttribute("src", documentUri, out var srcUri))
                    {
                        node.SetAttributeValue("src", srcUri.AbsoluteUri);
                    }
                    else
                    {
                        node.Attributes.Remove("src");
                    }
                }
            }
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
