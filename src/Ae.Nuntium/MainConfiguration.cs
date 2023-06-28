namespace Ae.Nuntium
{
    public sealed class MainConfiguration
    {
        public ConfiguredType SeleniumDriver { get; set; }
        public IList<ConfigurationItem> Finders { get; set; } = new List<ConfigurationItem>();
    }
}