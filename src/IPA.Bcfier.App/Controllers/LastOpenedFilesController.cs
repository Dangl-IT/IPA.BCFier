using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Models.Controllers.LastOpenedFiles;
using IPA.Bcfier.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/last-opened-files")]
    public class LastOpenedFilesController : ControllerBase
    {
        private readonly BcfierDbContext _context;
        private readonly SettingsService _settingsService;

        public LastOpenedFilesController(BcfierDbContext context,
            SettingsService settingsService)
        {
            _context = context;
            _settingsService = settingsService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(LastOpenedFilesWrapperGet), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLastOpenedFilesAsync([FromQuery]Guid? projectId)
        {
            var userName = (await _settingsService.LoadSettingsAsync()).Username;

            var lastOpenedFiles = await _context
                .LastOpenedUserFiles
                .Where(louf => louf.UserName == userName && louf.ProjectId == projectId)
                .OrderByDescending(louf => louf.OpenedAtAtUtc)
                .Select(louf => new LastOpenedFileGet
                {
                    FileName = louf.FilePath,
                    OpenedAtUtc = louf.OpenedAtAtUtc
                })
                .Take(10)
                .ToListAsync();

            return Ok(new LastOpenedFilesWrapperGet
            {
                LastOpenedFiles = lastOpenedFiles
            });
        }

        [HttpPut("")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> SetFileAsLastOpened([FromQuery]Guid? projectId, [FromQuery, Required]string filePath)
        {
            var userName = (await _settingsService.LoadSettingsAsync()).Username;
            var existingEntry = await _context.LastOpenedUserFiles
                .FirstOrDefaultAsync(louf => louf.ProjectId == projectId
                    && louf.UserName == userName
                    && louf.FilePath == filePath);
            if (existingEntry != null)
            {
                existingEntry.OpenedAtAtUtc = DateTimeOffset.UtcNow;
            }
            else
            {
                _context.LastOpenedUserFiles.Add(new Data.Models.LastOpenedUserFile
                {
                    ProjectId = projectId,
                    UserName = userName,
                    FilePath = filePath
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
