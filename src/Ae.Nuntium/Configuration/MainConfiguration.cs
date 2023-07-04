namespace Ae.Nuntium.Configuration
{
    public sealed class MainConfiguration
    {
        public ConfiguredType SeleniumDriver { get; set; }
        public IList<PipelineConfiguration> Pipelines { get; set; } = new List<PipelineConfiguration>();
    }
}