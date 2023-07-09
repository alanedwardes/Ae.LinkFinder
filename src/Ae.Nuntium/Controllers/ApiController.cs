using Ae.Nuntium.Configuration;
using Ae.Nuntium.Sources;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Ae.Nuntium.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly MainConfiguration _mainConfiguration;
        private readonly IPipelineServiceFactory _serviceFactory;

        public ApiController(MainConfiguration mainConfiguration, IPipelineServiceFactory serviceFactory)
        {
            _mainConfiguration = mainConfiguration;
            _serviceFactory = serviceFactory;
        }

        public sealed class TestExtractorBody
        {
            public SourceDocument Source { get; set; }
            public object Configuration { get; set; }
        }

        [HttpPost("extractors/{extractorType}/test")]
        public async Task<IActionResult> TestExtractor([FromRoute] string extractorType, [FromBody] TestExtractorBody body)
        {
            var jsonConfiguration = JsonSerializer.Serialize(body);

            var configuration = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(jsonConfiguration)))
                .Build();

            var extractor = _serviceFactory.GetExtractor(new ConfiguredType
            {
                Type = extractorType,
                Configuration = configuration.GetRequiredSection("Configuration")
            });

            var extractedPosts = await extractor.ExtractPosts(body.Source);
            return Json(extractedPosts);
        }
    }
}
