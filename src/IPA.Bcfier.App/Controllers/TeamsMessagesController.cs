using IPA.Bcfier.App.Models.Controllers.TeamsMessages;
using IPA.Bcfier.App.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IPA.Bcfier.App.Controllers
{
    [ApiController]
    [Route("api/teams-messages")]
    public class TeamsMessagesController : ControllerBase
    {
        private readonly TeamsMessagesService _teamsMessagesService;

        public TeamsMessagesController(TeamsMessagesService teamsMessagesService)
        {
            _teamsMessagesService = teamsMessagesService;
        }

        [HttpPost("projects/{projectId}/topics/{topicId}/comments")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> AnnounceNewCommentInProjectTopicAsync(Guid projectId, Guid topicId, [FromBody] TeamsMessagePost model)
        {
            await _teamsMessagesService.AnnounceNewCommentInProjectTopicAsync(projectId, model);
            return NoContent();
        }
    }
}
