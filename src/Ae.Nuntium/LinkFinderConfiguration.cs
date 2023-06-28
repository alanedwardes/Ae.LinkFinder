namespace Ae.Nuntium
{
    public sealed class LinkFinderConfiguration
    {
        public string Test { get; set; }
        public IList<string> Tests { get; set; } = new List<string>();
        public IList<LinkFinderConfigurationItem> Finders { get; set; } = new List<LinkFinderConfigurationItem>();
    }
}