using Dangl.Data.Shared;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Data.Models;
using IPA.Bcfier.App.Models.Controllers.ProjectUsers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}")]
    public class ProjectUsersController : ControllerBase
    {
        private readonly BcfierDbContext _context;

        public ProjectUsersController(BcfierDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(List<ProjectUserGet>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProjectUsersForProjectAsync(Guid projectId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound();
            }

            var projectUsers = await _context
                .ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Select(p => new ProjectUserGet
                {
                    Id = p.Id,
                    Identifier = p.Identifier
                })
                .ToListAsync();
            return Ok(projectUsers);
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(List<ProjectUserGet>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddUserToProjectAsync(Guid projectId, [FromBody] ProjectUserPost model)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound();
            }

            var identifierExistsAlready = await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == projectId && pu.Identifier == model.Identifier);
            if (identifierExistsAlready)
            {
                return BadRequest(new ApiError("This user already exists."));
            }

            var projectUser = new ProjectUser
            {
                Identifier = model.Identifier,
                ProjectId = projectId
            };
            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            var projectUsers = await _context
                .ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Select(p => new ProjectUserGet
                {
                    Id = p.Id,
                    Identifier = p.Identifier
                })
                .ToListAsync();
            return Ok(projectUsers);
        }

        [HttpDelete("{projectUserId}")]
        [ProducesResponseType(typeof(List<ProjectUserGet>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteProjectUserAsync(Guid projectId, Guid projectUserId)
        {
            var dbProjectUser = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.Id == projectUserId && pu.ProjectId == projectId);
            if (dbProjectUser == null)
            {
                return NotFound();
            }

            _context.ProjectUsers.Remove(dbProjectUser);
            await _context.SaveChangesAsync();

            var projectUsers = await _context
                .ProjectUsers
                .Where(pu => pu.ProjectId == projectId)
                .Select(p => new ProjectUserGet
                {
                    Id = p.Id,
                    Identifier = p.Identifier
                })
                .ToListAsync();
            return Ok(projectUsers);
        }
    }
}
