using IPA.Bcfier.App.Data;
using IPA.Bcfier.Models.Settings;
using IPA.Bcfier.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _settingsService;
        private readonly IServiceProvider _serviceProvider;
        private static string? _lastInitializedDatabaseLocation;

        public SettingsController(SettingsService settingsService,
            IServiceProvider serviceProvider)
        {
            _settingsService = settingsService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(Settings), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSettingsAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            if (!string.IsNullOrWhiteSpace(settings.MainDatabaseLocation) && settings.MainDatabaseLocation != _lastInitializedDatabaseLocation)
            {
                _lastInitializedDatabaseLocation = settings.MainDatabaseLocation;
                var dbContext = _serviceProvider.GetRequiredService<BcfierDbContext>();
                await dbContext.Database.MigrateAsync();
            }

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
