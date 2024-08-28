using Dangl.Data.Shared;
using Dangl.Data.Shared.QueryUtilities;
using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Data.Models;
using IPA.Bcfier.App.Models.Controllers.Users;
using LightQuery.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly BcfierDbContext _context;

        public UsersController(BcfierDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [AsyncLightQuery(forcePagination: true)]
        [ProducesResponseType(typeof(List<UserGet>), (int)HttpStatusCode.OK)]
        public IActionResult GetUsers(string? filter = null)
        {
            var dbUsers = _context
                .Users
                .Select(u => new UserGet
                {
                    Id = u.Id,
                    Identifier = u.Identifier
                });

            if (!string.IsNullOrWhiteSpace(filter))
            {
                dbUsers = dbUsers
                    .Filter(filter, text => u => EF.Functions.Like(u.Identifier, $"%{text}%"), transformFilterToLowercase: true);
            }
            return Ok(dbUsers);
        }

        [HttpGet("all")]
        [AsyncLightQuery(forcePagination: true)]
        [ProducesResponseType(typeof(List<UserGet>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var dbUsers = await _context
                .Users
                .Select(u => new UserGet
                {
                    Id = u.Id,
                    Identifier = u.Identifier
                })
                .ToListAsync();
            return Ok(dbUsers);
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UserGet), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateUserAsync([FromQuery] string userName)
        {
            var usernameExists = await _context
                .Users
                .AnyAsync(u => u.Identifier == userName);
            if (usernameExists)
            {
                return BadRequest(new ApiError("The username already exists"));
            }

            var newUser = new User
            {
                Identifier = userName
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new UserGet
            {
                Id = newUser.Id,
                Identifier = newUser.Identifier
            });
        }

        [HttpDelete("{userId}")]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new ApiError("The user does not exist"));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
