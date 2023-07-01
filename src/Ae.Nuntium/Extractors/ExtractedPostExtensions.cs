namespace Ae.Nuntium.Extractors
{
    public static class ExtractedPostExtensions
    {
        public static bool HasContent(this ExtractedPost post)
        {
            return post.Title != null || post.Permalink != null || post.TextSummary != null || post.Media.Count > 0;
        }
    }
}
