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
                    Identifier = p.User!.Identifier
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
            if (model.UserId == null && model.Identifier == null)
            {
                return BadRequest(new ApiError("Either UserId or Identifier must be provided."));
            }

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
            {
                return NotFound();
            }

            var identifierExistsAlready = await _context.ProjectUsers.AnyAsync(pu => pu.ProjectId == projectId
                && pu.UserId == model.UserId);
            if (identifierExistsAlready)
            {
                return BadRequest(new ApiError("This user assignment already exists."));
            }

            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId
            || u.Identifier == model.Identifier);
            if (dbUser == null)
            {
                return BadRequest(new ApiError("The user does not exist."));
            }

            var projectUser = new ProjectUser
            {
                UserId = dbUser.Id,
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
                    Identifier = dbUser.Identifier
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
                    Identifier = p.User.Identifier
                })
                .ToListAsync();
            return Ok(projectUsers);
        }
    }
}
