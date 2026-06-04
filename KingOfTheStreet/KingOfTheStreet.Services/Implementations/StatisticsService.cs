using KingOfTheStreet.Data;
using KingOfTheStreet.Services.Interfaces;
using KingOfTheStreet.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;
        public StatisticsService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<LeaderRow>> GetTopScorersAsync(int count) =>
            await AggregateAsync(s => s.Points, count);

        public async Task<IEnumerable<LeaderRow>> GetTopRebloundersAsync(int count) =>
            await AggregateAsync(s => s.Rebounds, count);

        public async Task<IEnumerable<LeaderRow>> GetTopAssistLeadersAsync(int count) =>
            await AggregateAsync(s => s.Assists, count);

        public async Task<IEnumerable<LeaderRow>> GetBestDefendersAsync(int count) =>
            await AggregateAsync(s => s.Steals + s.Blocks, count);

        private async Task<IEnumerable<LeaderRow>> AggregateAsync(
            Func<Data.Models.PlayerMatchStat, int> selector, int count)
        {
            var stats = await _context.PlayerMatchStats
                .Include(s => s.Player).ThenInclude(p => p.Team)
                .ToListAsync();

            return stats
                .GroupBy(s => s.PlayerId)
                .Select(g => new LeaderRow
                {
                    PlayerId = g.Key,
                    PlayerName = g.First().Player?.FullName ?? "Unknown",
                    TeamName = g.First().Player?.Team?.Name ?? "Free Agent",
                    Value = g.Sum(selector),
                    GamesPlayed = g.Count()
                })
                .OrderByDescending(r => r.Value)
                .Take(count)
                .ToList();
        }

        public async Task<IEnumerable<LeaderRow>> GetMvpRankingAsync(int tournamentId, int count)
        {
            var votes = await _context.MVPVotes
                .Include(v => v.Player).ThenInclude(p => p.Team)
                .Where(v => v.TournamentId == tournamentId)
                .ToListAsync();

            return votes
                .GroupBy(v => v.PlayerId)
                .Select(g => new LeaderRow
                {
                    PlayerId = g.Key,
                    PlayerName = g.First().Player?.FullName ?? "Unknown",
                    TeamName = g.First().Player?.Team?.Name ?? "Free Agent",
                    Value = g.Count()
                })
                .OrderByDescending(r => r.Value)
                .Take(count)
                .ToList();
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            return new DashboardStats
            {
                TotalTournaments = await _context.Tournaments.CountAsync(),
                TotalTeams = await _context.Teams.CountAsync(),
                TotalPlayers = await _context.Players.CountAsync(),
                UpcomingMatches = await _context.Matches
                    .CountAsync(m => m.Status == Data.Models.MatchStatus.Scheduled),
                TotalUsers = await _context.Users.CountAsync()
            };
        }
    }
}
