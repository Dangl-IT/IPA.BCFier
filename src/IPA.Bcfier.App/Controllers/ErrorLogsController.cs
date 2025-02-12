using IPA.Bcfier.App.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/error-logs")]
    public class ErrorLogsController : ControllerBase
    {
        private readonly ErrorLogsService _errorLogsService;

        public ErrorLogsController(ErrorLogsService errorLogsService)
        {
            _errorLogsService = errorLogsService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetErrorLogAsync()
        {
            var errorLogs = await _errorLogsService.GetErrorLogAsync();
            return Content(errorLogs!.ToString(), "application/json");
        }
    }
}
