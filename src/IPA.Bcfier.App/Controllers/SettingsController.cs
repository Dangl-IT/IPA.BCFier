using ElectronNET.API;
using ElectronNET.API.Entities;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Services;
using IPA.Bcfier.Models.Settings;
using IPA.Bcfier.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Text;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _settingsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ElectronWindowProvider _electronWindowProvider;
        private static string? _lastInitializedDatabaseLocation;

        public SettingsController(SettingsService settingsService,
            IServiceProvider serviceProvider,
            ElectronWindowProvider electronWindowProvider)
        {
            _settingsService = settingsService;
            _serviceProvider = serviceProvider;
            _electronWindowProvider = electronWindowProvider;
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

        [HttpGet("database-location")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ChoseMainDatabaseLocationAsync()
        {
            var electronWindow = _electronWindowProvider.BrowserWindow;
            if (electronWindow == null)
            {
                return BadRequest();
            }
            var fileSaveSelectResult = await Electron.Dialog.ShowSaveDialogAsync(electronWindow, new SaveDialogOptions
            {
                DefaultPath = "IPA.bcfierdb",
                Filters = new[]
                {
                    new FileFilter
                    {
                        Name = "BCFier DB",
                        Extensions = new string[] { "bcfierdb" }
                    }
                }
            });

            if (fileSaveSelectResult != null && !string.IsNullOrWhiteSpace(fileSaveSelectResult))
            {
                var currentSettings = await _settingsService.LoadSettingsAsync();
                currentSettings.MainDatabaseLocation = fileSaveSelectResult;
                await _settingsService.SaveSettingsAsync(currentSettings);
            }

            return NoContent();
        }


        [HttpPut("")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SaveSettingsAsync([FromBody] Settings settings)
        {
            await _settingsService.SaveSettingsAsync(settings);
            return NoContent();
        }

        [HttpGet("always-on-top")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIsAlwaysOnTopAsync()
        {
            var electronWindow = _electronWindowProvider.BrowserWindow;
            if (electronWindow == null)
            {
                return BadRequest();
            }

            var isAlwaysOnTop = await electronWindow.IsAlwaysOnTopAsync();
            return Ok(isAlwaysOnTop);
        }

        [HttpPut("always-on-top")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SetIsAlwaysOnTopAsync(bool isAlwaysOnTop)
        {
            var electronWindow = _electronWindowProvider.BrowserWindow;
            if (electronWindow == null)
            {
                return BadRequest();
            }

            electronWindow.SetAlwaysOnTop(isAlwaysOnTop);
            return NoContent();
        }
    }
}
