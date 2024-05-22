using IPA.Bcfier.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using IPA.Bcfier.App.Configuration;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/frontend-config")]
    public class FrontendConfigController : ControllerBase
    {
        private static string? _frontendConfig;
        private readonly RevitParameters _revitParameters;
        private readonly NavisworksParameters _navisworksParameters;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FrontendConfigController(RevitParameters revitParameters,
            NavisworksParameters navisworksParameters,
            IWebHostEnvironment webHostEnvironment)
        {
            _revitParameters = revitParameters;
            _navisworksParameters = navisworksParameters;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("config.js")]
        public IActionResult GetFrontendConfigAsJavaScript([FromQuery] string? timestamp)
        {
            if (!string.IsNullOrWhiteSpace(timestamp))
            {
                HttpContext.Response
                    .GetTypedHeaders()
                    .CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(365)
                    };
            }

            return GetContentResultForFrontendConfig();
        }

        [HttpGet("json")]
        [ProducesResponseType(typeof(FrontendConfig), (int)HttpStatusCode.OK)]
        public IActionResult GetFrontendConfig([FromQuery] string? timestamp)
        {
            if (!string.IsNullOrWhiteSpace(timestamp))
            {
                HttpContext.Response
                    .GetTypedHeaders()
                    .CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(365)
                    };
            }


            if (_frontendConfig == null)
            {
                InitializeFrontendConfig();
            }

            return Ok(_frontendConfig);
        }

        private ContentResult GetContentResultForFrontendConfig()
        {
            if (_frontendConfig == null)
            {
                InitializeFrontendConfig();
            }

            return Content(_frontendConfig!, "application/javascript");
        }

        private void InitializeFrontendConfig()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var frontendConfigObject = GetFrontendConfigModel();
            var serializedConfig = JsonConvert.SerializeObject(frontendConfigObject, serializerSettings);
            _frontendConfig = @"(function() {
window.ipaBcfierFrontendConfig = "
+ serializedConfig
+ @";
})();";
        }

        private FrontendConfig GetFrontendConfigModel()
        {
            return new FrontendConfig
            {
                IsInElectronMode = true,
                IsConnectedToRevit = _revitParameters.IsConnectedToRevit,
                IsConnectedToNavisworks = _navisworksParameters.IsConnectedToNavisworks,
                RevitProjectPath = _revitParameters.RevitProjectPath,
                Environment = _webHostEnvironment.EnvironmentName
            };
        }
    }
}
