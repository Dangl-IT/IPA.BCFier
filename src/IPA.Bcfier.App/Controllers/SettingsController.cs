using IPA.Bcfier.Models.Settings;
using IPA.Bcfier.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _settingsService;

        public SettingsController(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(Settings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSettingsAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            return Ok(settings);
        }

        [HttpPut("")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SaveSettingsAsync([FromBody] Settings settings)
        {
            await _settingsService.SaveSettingsAsync(settings);
            return NoContent();
        }
    }
}
