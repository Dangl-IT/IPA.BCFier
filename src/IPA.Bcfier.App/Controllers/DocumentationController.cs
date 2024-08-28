using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/documentation")]
    public class DocumentationController : ControllerBase
    {
        [HttpPost("")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> OpenDocumentation()
        {
            await Electron.Shell.OpenExternalAsync("https://docs.dangl-it.com/Projects/IPA.BCFier");
            return NoContent();
        }
    }
}
