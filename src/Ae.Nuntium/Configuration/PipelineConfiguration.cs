namespace Ae.Nuntium.Configuration
{
    public sealed class PipelineConfiguration
    {
        public bool Skip { get; set; }
        public bool Testing { get; set; }
        public string Cron { get; set; }
        public int JitterSeconds { get; set; }
        public ConfiguredType Source { get; set; } = new ConfiguredType();
        public ConfiguredType Extractor { get; set; } = new ConfiguredType();
        public ConfiguredType Tracker { get; set; } = new ConfiguredType();
        public ConfiguredType? Enricher { get; set; }
        public IList<ConfiguredType> Destinations { get; set; } = new List<ConfiguredType>();
    }
}