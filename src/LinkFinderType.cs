using Microsoft.Extensions.Configuration;

namespace Ae.LinkFinder
{
    public sealed class LinkFinderType
    {
        public string Type { get; set; }
        public IConfigurationSection Configuration { get; set; }
    }
}