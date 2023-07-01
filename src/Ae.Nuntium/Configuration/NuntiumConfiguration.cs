namespace Ae.Nuntium.Configuration
{
    public sealed class NuntiumConfiguration
    {
        public ConfiguredType SeleniumDriver { get; set; }
        public IList<NuntiumFinder> Finders { get; set; } = new List<NuntiumFinder>();
    }
}