using Microsoft.Extensions.Configuration;

namespace Ae.Nuntium.Configuration
{
    public sealed class ConfiguredType
    {
        public string Type { get; set; }
        public IConfigurationSection Configuration { get; set; }
    }
}