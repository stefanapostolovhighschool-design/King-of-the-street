using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers.Api
{
    [ApiController]
    [Route("api/matches")]
    public class MatchesApiController : ControllerBase
    {
        private readonly IMatchService _matches;
        private readonly IBracketService _bracket;

        public MatchesApiController(IMatchService matches, IBracketService bracket)
        {
            _matches = matches;
            _bracket = bracket;
        }

        // GET api/matches/live/{tournamentId}
        [HttpGet("live/{tournamentId:int}")]
        public async Task<IActionResult> Live(int tournamentId)
        {
            var matches = await _matches.GetLiveAsync(tournamentId);
            var payload = matches.Select(m => new
            {
                id = m.Id,
                teamA = m.TeamA?.Name ?? "TBD",
                teamB = m.TeamB?.Name ?? "TBD",
                scoreA = m.ScoreA,
                scoreB = m.ScoreB,
                status = m.Status.ToString(),
                round = m.Round
            });
            return Ok(payload);
        }

        // GET api/matches/bracket/{tournamentId}
        [HttpGet("bracket/{tournamentId:int}")]
        public async Task<IActionResult> Bracket(int tournamentId)
        {
            var matches = await _bracket.GetBracketAsync(tournamentId);
            var rounds = matches
                .GroupBy(m => m.Round)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    round = g.Key,
                    matches = g.Select(m => new
                    {
                        id = m.Id,
                        teamA = m.TeamA?.Name ?? "TBD",
                        teamB = m.TeamB?.Name ?? "BYE",
                        scoreA = m.ScoreA,
                        scoreB = m.ScoreB,
                        winner = m.Winner?.Name,
                        status = m.Status.ToString()
                    })
                });
            return Ok(rounds);
        }

        // POST api/matches/result  (Admin only)
        [HttpPost("result")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Result([FromBody] MatchResultDto dto)
        {
            if (dto == null || dto.MatchId <= 0)
                return BadRequest(new { error = "Invalid match result payload." });

            await _matches.EnterResultAsync(dto.MatchId, dto.ScoreA, dto.ScoreB);
            return Ok(new { success = true, dto.MatchId, dto.ScoreA, dto.ScoreB });
        }

        public class MatchResultDto
        {
            public int MatchId { get; set; }
            public int ScoreA { get; set; }
            public int ScoreB { get; set; }
        }
    }
}
