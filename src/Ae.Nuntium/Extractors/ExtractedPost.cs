namespace Ae.Nuntium.Extractors
{
    public sealed class ExtractedPost
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public Uri? Permalink { get; set; }
        public ISet<Uri> Links { get; set; } = new HashSet<Uri>();
        public ISet<Uri> Media { get; set; } = new HashSet<Uri>();
        public override string ToString() => $"{Permalink} {Author}: {Content}";
    }
}
