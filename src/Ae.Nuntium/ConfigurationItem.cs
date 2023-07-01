﻿namespace Ae.Nuntium
{
    public sealed class ConfigurationItem
    {
        public bool Testing { get; set; }
        public string Cron { get; set; }
        public ConfiguredType Source { get; set; } = new ConfiguredType();
        public ConfiguredType Extractor { get; set; } = new ConfiguredType();
        public ConfiguredType Tracker { get; set; } = new ConfiguredType();
        public IList<ConfiguredType> Destinations { get; set; } = new List<ConfiguredType>();
    }
}