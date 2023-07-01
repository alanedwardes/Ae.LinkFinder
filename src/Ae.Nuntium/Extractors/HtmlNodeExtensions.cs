using HtmlAgilityPack;

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
    }
}
