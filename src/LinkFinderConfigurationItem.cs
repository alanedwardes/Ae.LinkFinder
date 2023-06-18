namespace Ae.LinkFinder
{
    public sealed class LinkFinderConfigurationItem
    {
        public bool Testing { get; set; }
        public string Cron { get; set; }
        public LinkFinderType Source { get; set; } = new LinkFinderType();
        public LinkFinderType Tracker { get; set; } = new LinkFinderType();
        public IList<LinkFinderType> Destinations { get; set; } = new List<LinkFinderType>();
    }
}