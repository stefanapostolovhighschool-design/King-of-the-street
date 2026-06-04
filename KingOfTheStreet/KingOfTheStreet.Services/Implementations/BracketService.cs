using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class BracketService : IBracketService
    {
        private readonly ApplicationDbContext _context;
        public BracketService(ApplicationDbContext context) => _context = context;

        public async Task GenerateBracketAsync(int tournamentId, int? seed = null)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null)
                throw new InvalidOperationException("Tournament not found.");

            
            var approvedTeamIds = await _context.TournamentRegistrations
                .Where(r => r.TournamentId == tournamentId && r.IsApproved)
                .Select(r => r.TeamId)
                .ToListAsync();

            List<Team> teams;
            if (approvedTeamIds.Any())
                teams = await _context.Teams.Where(t => approvedTeamIds.Contains(t.Id)).ToListAsync();
            else
                teams = await _context.Teams.Where(t => t.IsApproved).ToListAsync();

            if (teams.Count < 2)
                throw new InvalidOperationException("At least two approved teams are required to generate a bracket.");

           
            var rnd = seed.HasValue ? new Random(seed.Value) : new Random();
            teams = teams.OrderBy(_ => rnd.Next()).ToList();

            
            var old = _context.Matches.Where(m => m.TournamentId == tournamentId);
            _context.Matches.RemoveRange(old);
            await _context.SaveChangesAsync();

            var courts = await _context.Courts.ToListAsync();

          
            int round = 1;
            for (int i = 0; i < teams.Count; i += 2)
            {
                var teamA = teams[i];
                var teamB = (i + 1 < teams.Count) ? teams[i + 1] : null; 

                var match = new Match
                {
                    TournamentId = tournamentId,
                    TeamAId = teamA.Id,
                    TeamBId = teamB?.Id,
                    Round = round,
                    MatchDate = tournament.StartDate.AddHours(i),
                    CourtId = courts.Count > 0 ? courts[(i / 2) % courts.Count].Id : (int?)null,
                    Status = MatchStatus.Scheduled
                };

                
                if (teamB == null)
                {
                    match.Status = MatchStatus.Finished;
                    match.WinnerId = teamA.Id;
                }

                _context.Matches.Add(match);
            }

            tournament.Status = TournamentStatus.InProgress;
            await _context.SaveChangesAsync();
        }

        public async Task<int?> AdvanceRoundAsync(int tournamentId)
        {
            var matches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();

            int currentRound = matches.Max(m => m.Round);
            var currentRoundMatches = matches.Where(m => m.Round == currentRound).ToList();

            
            if (currentRoundMatches.Any(m => m.Status != MatchStatus.Finished))
                throw new InvalidOperationException("All matches in the current round must be finished first.");

            var winners = currentRoundMatches
                .Where(m => m.WinnerId.HasValue)
                .Select(m => m.WinnerId.Value)
                .ToList();

        
            if (winners.Count <= 1)
            {
                var tournament = await _context.Tournaments.FindAsync(tournamentId);
                if (tournament != null)
                {
                    tournament.Status = TournamentStatus.Completed;
                    await _context.SaveChangesAsync();
                }
                return winners.FirstOrDefault();
            }

            
            int nextRound = currentRound + 1;
            var courts = await _context.Courts.ToListAsync();
            for (int i = 0; i < winners.Count; i += 2)
            {
                var match = new Match
                {
                    TournamentId = tournamentId,
                    TeamAId = winners[i],
                    TeamBId = (i + 1 < winners.Count) ? winners[i + 1] : (int?)null,
                    Round = nextRound,
                    MatchDate = DateTime.UtcNow.AddDays(1).AddHours(i),
                    CourtId = courts.Count > 0 ? courts[(i / 2) % courts.Count].Id : (int?)null,
                    Status = MatchStatus.Scheduled
                };
                if (match.TeamBId == null)
                {
                    match.Status = MatchStatus.Finished;
                    match.WinnerId = winners[i];
                }
                _context.Matches.Add(match);
            }
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<IEnumerable<Match>> GetBracketAsync(int tournamentId) =>
            await _context.Matches
                .Include(m => m.TeamA).Include(m => m.TeamB).Include(m => m.Winner)
                .Where(m => m.TournamentId == tournamentId)
                .OrderBy(m => m.Round).ThenBy(m => m.Id)
                .ToListAsync();
    }
}
