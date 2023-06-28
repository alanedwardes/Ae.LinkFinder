using Microsoft.Extensions.Configuration;

namespace Ae.Nuntium
{
    public sealed class LinkFinderType
    {
        public string Type { get; set; }
        public IConfigurationSection Configuration { get; set; }
    }
}