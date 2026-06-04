using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using KingOfTheStreet.Services.Models;
using KingOfTheStreet.Services.Simulation;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class MatchService : IMatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMatchSimulator _simulator;

        public MatchService(ApplicationDbContext context, IMatchSimulator simulator)
        {
            _context = context;
            _simulator = simulator;
        }

        public async Task<IEnumerable<Match>> GetScheduleAsync(int tournamentId) =>
            await _context.Matches
                .Include(m => m.TeamA).Include(m => m.TeamB).Include(m => m.Court)
                .Where(m => m.TournamentId == tournamentId)
                .OrderBy(m => m.Round).ThenBy(m => m.MatchDate)
                .ToListAsync();

        public async Task<Match> GetByIdAsync(int id) =>
            await _context.Matches
                .Include(m => m.TeamA).Include(m => m.TeamB)
                .Include(m => m.Court).Include(m => m.MvpPlayer)
                .Include(m => m.PlayerStats).ThenInclude(s => s.Player)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<IEnumerable<Match>> GetLiveAsync(int tournamentId) =>
            await _context.Matches
                .Include(m => m.TeamA).Include(m => m.TeamB)
                .Where(m => m.TournamentId == tournamentId &&
                            (m.Status == MatchStatus.Live || m.Status == MatchStatus.Finished))
                .OrderByDescending(m => m.Status == MatchStatus.Live)
                .ThenByDescending(m => m.MatchDate)
                .ToListAsync();

        public async Task EnterResultAsync(int matchId, int scoreA, int scoreB)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match == null) return;

            match.ScoreA = scoreA;
            match.ScoreB = scoreB;
            match.Status = MatchStatus.Finished;
            match.WinnerId = scoreA >= scoreB ? match.TeamAId : match.TeamBId;
            await _context.SaveChangesAsync();
        }

        public async Task<MatchSimulationResult> SimulateMatchAsync(int matchId, int? seed = null)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA).ThenInclude(t => t.Players)
                .Include(m => m.TeamB).ThenInclude(t => t.Players)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match?.TeamA == null || match.TeamB == null)
                throw new InvalidOperationException("Match must have two teams assigned before simulation.");

            var result = _simulator.Simulate(match.TeamA, match.TeamB, seed);

         
            match.ScoreA = result.ScoreA;
            match.ScoreB = result.ScoreB;
            match.WinnerId = result.WinnerTeamId;
            match.MvpPlayerId = result.MvpPlayerId;
            match.Status = MatchStatus.Finished;

            
            var existing = _context.PlayerMatchStats.Where(s => s.MatchId == matchId);
            _context.PlayerMatchStats.RemoveRange(existing);

            foreach (var b in result.BoxScores)
            {
                _context.PlayerMatchStats.Add(new PlayerMatchStat
                {
                    MatchId = matchId,
                    PlayerId = b.PlayerId,
                    TeamId = b.TeamId,
                    Points = b.Points,
                    Assists = b.Assists,
                    Rebounds = b.Rebounds,
                    Steals = b.Steals,
                    Blocks = b.Blocks,
                    FieldGoalsMade = b.FieldGoalsMade,
                    FieldGoalsAttempted = b.FieldGoalsAttempted,
                    ThreePointersMade = b.ThreePointersMade,
                    ThreePointersAttempted = b.ThreePointersAttempted,
                    FreeThrowsMade = b.FreeThrowsMade,
                    FreeThrowsAttempted = b.FreeThrowsAttempted
                });
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<MatchSimulationResult>> SimulateTournamentAsync(int tournamentId, int? seed = null)
        {
            var matches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId && m.Status != MatchStatus.Finished)
                .OrderBy(m => m.Round)
                .Select(m => m.Id)
                .ToListAsync();

            var results = new List<MatchSimulationResult>();
            int offset = 0;
            foreach (var id in matches)
            {
                int? matchSeed = seed.HasValue ? seed.Value + offset++ : (int?)null;
                results.Add(await SimulateMatchAsync(id, matchSeed));
            }
            return results;
        }
    }
}
