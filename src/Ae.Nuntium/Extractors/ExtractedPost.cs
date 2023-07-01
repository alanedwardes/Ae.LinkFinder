namespace Ae.Nuntium.Extractors
{
    public sealed class ExtractedPost
    {
        /// <summary>
        /// An optional title.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// An optional created date/time in UTC.
        /// </summary>
        public DateTime? Published { get; set; }
        /// <summary>
        /// An optional author.
        /// </summary>
        public string? Author { get; set; }
        /// <summary>
        /// An optional short text version of the post content. This cannot contain HTML.
        /// </summary>
        public string? TextSummary { get; set; }
        /// <summary>
        /// The raw document, if available. This may be an HTML document.
        /// This cannot be set if <see cref="TextSummary"/> is not set.
        /// </summary>
        public string? RawContent { get; set; }
        /// <summary>
        /// An optional link to the post.
        /// </summary>
        public Uri? Permalink { get; set; }
        /// <summary>
        /// An optional set of links contained within the post.
        /// </summary>
        public ISet<Uri> Links { get; set; } = new HashSet<Uri>();
        /// <summary>
        /// An optional set of media contained within the post.
        /// </summary>
        public ISet<Uri> Media { get; set; } = new HashSet<Uri>();
        public override string ToString() => $"{Permalink} {Author}: {TextSummary}";
    }
}
