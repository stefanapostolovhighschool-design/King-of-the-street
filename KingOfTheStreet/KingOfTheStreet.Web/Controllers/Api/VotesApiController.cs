using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers.Api
{
    [ApiController]
    [Route("api/votes")]
    [Authorize] // Registered users only.
    public class VotesApiController : ControllerBase
    {
        private readonly IMvpVoteService _votes;
        private readonly UserManager<ApplicationUser> _userManager;

        public VotesApiController(IMvpVoteService votes, UserManager<ApplicationUser> userManager)
        {
            _votes = votes;
            _userManager = userManager;
        }

        // POST api/votes/mvp
        [HttpPost("mvp")]
        public async Task<IActionResult> Vote([FromBody] MvpVoteDto dto)
        {
            if (dto == null) return BadRequest(new { error = "Invalid payload." });

            var userId = _userManager.GetUserId(User);
            bool success = await _votes.VoteAsync(userId, dto.TournamentId, dto.PlayerId);
            int count = await _votes.GetVoteCountAsync(dto.PlayerId, dto.TournamentId);

            return Ok(new
            {
                success,
                count,
                message = success ? "Vote recorded!" : "You have already voted in this tournament."
            });
        }

        public class MvpVoteDto
        {
            public int TournamentId { get; set; }
            public int PlayerId { get; set; }
        }
    }
}
