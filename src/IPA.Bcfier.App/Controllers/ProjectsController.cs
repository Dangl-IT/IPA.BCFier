using Dangl.Data.Shared;
using Dangl.Data.Shared.QueryUtilities;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Data.Models;
using IPA.Bcfier.App.Models.Controllers.Projects;
using LightQuery.Client;
using LightQuery.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly BcfierDbContext _context;

        public ProjectsController(BcfierDbContext context)
        {
            _context = context;
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
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
