using IPA.Bcfier.App.Data;
using IPA.Bcfier.Services;
using Microsoft.EntityFrameworkCore;

namespace IPA.Bcfier.App.Services
{
    public class LastOpenedFilesService
    {
        private readonly BcfierDbContext _context;
        private readonly SettingsService _settingsService;

        public LastOpenedFilesService(BcfierDbContext context,
            SettingsService settingsService)
        {
            _context = context;
            _settingsService = settingsService;
        }

        public async Task SetFileAsLastOpenedAsync(Guid? projectId, string filePath)
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
        }
    }
}
