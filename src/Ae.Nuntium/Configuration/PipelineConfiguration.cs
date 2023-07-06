namespace Ae.Nuntium.Configuration
{
    public sealed class PipelineConfiguration
    {
        public bool Skip { get; set; }
        public bool Testing { get; set; }
        public string? Cron { get; set; }
        public int JitterSeconds { get; set; }
        public ConfiguredType? Source { get; set; }
        public ConfiguredType? Extractor { get; set; }
        public ConfiguredType? Tracker { get; set; }
        public ConfiguredType? Enricher { get; set; } = new ConfiguredType { Type = "WhitespaceRemoval" };
        public IList<ConfiguredType>? Enrichers { get; set; }
        public ConfiguredType? Destination { get; set; } = new ConfiguredType { Type = "Console" };
        public IList<ConfiguredType>? Destinations { get; set; }

        public IEnumerable<ConfiguredType> EnrichersMarshaled => GetConfiguredTypes(Enricher, Enrichers);
        public IEnumerable<ConfiguredType> DestinationsMarshaled => GetConfiguredTypes(Destination, Destinations);
        private static IEnumerable<ConfiguredType> GetConfiguredTypes(ConfiguredType? single, IEnumerable<ConfiguredType>? collection)
        {
            if (single != null)
            {
                yield return single;
            }

            foreach (var item in collection ?? Enumerable.Empty<ConfiguredType>())
            {
                yield return item;
            }
        }
    }
}