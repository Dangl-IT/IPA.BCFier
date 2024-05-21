using IPA.Bcfier.App.Data;
using IPA.Bcfier.App.Models.Controllers.ProjectUsers;
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
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var distinctUserNames = await _context
                .ProjectUsers
                .Select(pu => pu.Identifier)
                .Distinct()
                .ToListAsync();
            return Ok(distinctUserNames);
        }
    }
}
