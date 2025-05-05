using Dangl.Data.Shared;
using Dangl.Data.Shared.QueryUtilities;
using ElectronNET.API.Entities;
using ElectronNET.API;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Data.Models;
using IPA.Bcfier.App.Models.Controllers.Projects;
using IPA.Bcfier.App.Services;
using IPA.Bcfier.Ipc;
using IPA.Bcfier.Models.Projects;
using LightQuery.Client;
using LightQuery.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly BcfierDbContext _context;
        private readonly ElectronWindowProvider _electronWindowProvider;

        public ProjectsController(BcfierDbContext context, ElectronWindowProvider electronWindowProvider)
        {
            _context = context;
            _electronWindowProvider = electronWindowProvider;
        }

        [AsyncLightQuery(forcePagination: true)]
        [HttpGet("")]
        [ProducesResponseType(typeof(PaginationResult<ProjectGet>), (int)HttpStatusCode.OK)]
        public IActionResult GetAllProjects(string? filter = null, string? revitPathFilter = null)
        {
            var projectsQuery = _context
                .Projects
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                projectsQuery = projectsQuery
                    .Filter(filter, text => p => EF.Functions.Like(p.Name, $"%{text}%"), transformFilterToLowercase: true);
            }

            if (!string.IsNullOrWhiteSpace(revitPathFilter))
            {
                projectsQuery = projectsQuery
                    .Where(project => project.RevitIdentifer == revitPathFilter);
            }

            return Ok(projectsQuery.Select(p => new ProjectGet
            {
                Id = p.Id,
                Name = p.Name,
                RevitIdentifier = p.RevitIdentifer,
                TeamsWebhook = p.TeamsWebhook,
                CreatedAtUtc = p.CreatedAtUtc
            }));
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(ProjectGet), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateProjectAsync(ProjectPost model)
        {
            var project = new Project
            {
                Name = model.Name,
                Number = model.Number,
                FilePath = model.FilePath,
                RevitIdentifer = model.RevitIdentifier ?? string.Empty,
                TeamsWebhook = model.TeamsWebhook
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return Ok(new ProjectGet
            {
                Id = project.Id,
                Name = project.Name,
                RevitIdentifier = project.RevitIdentifer,
                TeamsWebhook = project.TeamsWebhook,
                CreatedAtUtc = project.CreatedAtUtc
            });
        }

        [HttpPut("{projectId}")]
        [ProducesResponseType(typeof(ProjectGet), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> EditProjectAsync(Guid projectId, ProjectPut model)
        {
            var dbProject = await _context
                .Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (dbProject == null)
            {
                return BadRequest(new ApiError("There is no project with the given id."));
            }

            dbProject.Name = model.Name;
            dbProject.RevitIdentifer = model.RevitIdentifier ?? string.Empty;
            dbProject.TeamsWebhook = model.TeamsWebhook;

            await _context.SaveChangesAsync();
            return Ok(new ProjectGet
            {
                Id = dbProject.Id,
                Name = dbProject.Name,
                RevitIdentifier = dbProject.RevitIdentifer,
                TeamsWebhook = dbProject.TeamsWebhook,
                CreatedAtUtc = dbProject.CreatedAtUtc
            });
        }

        [HttpDelete("{projectId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteProjectAsync(Guid projectId)
        {
            var dbProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (dbProject == null)
            {
                return BadRequest(new ApiError("There is no project with the given id."));
            }

            _context.Projects.Remove(dbProject);

            var lastOpenedFiles = await _context.LastOpenedUserFiles
                .Where(f => f.ProjectId == projectId)
                .ToListAsync();
            _context.LastOpenedUserFiles.RemoveRange(lastOpenedFiles);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("project-number-and-file-path")]
        [ProducesResponseType(typeof(ProjectData), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetProjectNumberAndFilePathAsync()
        {
            var hasReceived = false;
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < TimeSpan.FromSeconds(120) && !hasReceived)
            {
                if (IpcHandler.ReceivedMessages.TryDequeue(out var message))
                {
                    var ipcMessage = JsonConvert.DeserializeObject<IpcMessage>(message)!;
                    if (ipcMessage.Command == IpcMessageCommand.GetProjectNumberAndFilePath)
                    {
                        if (string.IsNullOrWhiteSpace(ipcMessage.Data))
                        {
                            break;
                        }
                        else
                        {
                            hasReceived = true;
                            IpcHandler.ReceivedMessages.Enqueue(message);
                            return Ok(JsonConvert.DeserializeObject<ProjectData>(ipcMessage.Data));
                        }
                    }
                    else
                    {
                        IpcHandler.ReceivedMessages.Enqueue(message);
                        await Task.Delay(100);
                    }
                }
            }

            return BadRequest();
        }

        [HttpGet("project-location")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChoseProjectLocationAsync()
        {
            var electronWindow = _electronWindowProvider.BrowserWindow;
            if (electronWindow == null)
            {
                return BadRequest();
            }

            var dialogOptions = new OpenDialogOptions
            {
                Title = "Select a folder",
                Properties = new[] { OpenDialogProperty.openDirectory },
                DefaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            var result = await Electron.Dialog.ShowOpenDialogAsync(electronWindow, dialogOptions);
            if (result == null || result.Count() == 0)
            {
                return BadRequest();
            }

            return Ok(result[0]);
        }
    }
}
