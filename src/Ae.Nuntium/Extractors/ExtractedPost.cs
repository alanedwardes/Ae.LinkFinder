namespace Ae.Nuntium.Extractors
{
    public sealed class ExtractedPost
    {
        public ExtractedPost(Uri permalink) => Permalink = permalink ?? throw new ArgumentNullException(nameof(permalink));

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
        /// The optional avatar of the author.
        /// </summary>
        public Uri? Avatar { get; set; }
        /// <summary>
        /// An optional main thumbnail uri for the post.
        /// </summary>
        public Uri? Thumbnail { get; set; }
        /// <summary>
        /// An optional short markdown or text version of the post content. This cannot contain HTML.
        /// </summary>
        public string? TextSummary { get; set; }
        /// <summary>
        /// The raw document, if available. This may be an HTML document.
        /// This cannot be set if <see cref="TextSummary"/> is not set.
        /// </summary>
        public string? RawContent { get; set; }
        /// <summary>
        /// A required link to the post. This can also be a URN, for non-HTTP sources (just must be a non-null <see cref="Uri"/>).
        /// </summary>
        public Uri Permalink { get; set; }
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
