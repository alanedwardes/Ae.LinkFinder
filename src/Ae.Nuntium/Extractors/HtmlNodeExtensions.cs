using Ae.Nuntium.Services;
using HtmlAgilityPack;
using System.Web;

namespace Ae.Nuntium.Extractors
{
    public static class HtmlNodeExtensions
    {
        public static void GetLinksAndMedia(this HtmlNode parent, Uri documentUri, Action<Uri> foundLink, Action<Uri> foundMedia)
        {
            foreach (var node in parent.DescendantsAndSelf())
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
        public static bool TryGetTextFromAttribute(this HtmlNode node, string attributeName, out string text)
        {
            if (node == null)
            {
                text = null;
                return false;
            }

            var attributeValue = node.GetAttributeValue<string>(attributeName, null);
            if (attributeValue == null)
            {
                text = null;
                return false;
            }

            text = HttpUtility.HtmlDecode(attributeValue.Trim());
            return true;
        }

        public static bool TryGetAbsoluteUriFromAttribute(this HtmlNode node, string attributeName, out Uri newUri)
        {
            if (TryGetTextFromAttribute(node, attributeName, out var attributeValue))
            {
                return Uri.TryCreate(HttpUtility.HtmlDecode(attributeValue.Trim()), UriKind.Absolute, out newUri);
            }

            newUri = null;
            return false; 
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

            return UriExtensions.TryCreateAbsoluteUri(attributeValue.Trim(), baseAddress, out newUri);
        }

        public static void MakeRelativeUrisAbsolute(this HtmlNode parent, Uri documentUri)
        {
            foreach (var node in parent.DescendantsAndSelf())
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
    }
}
