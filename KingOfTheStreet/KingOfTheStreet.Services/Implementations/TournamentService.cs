using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class TournamentService : ITournamentService
    {
        private readonly ApplicationDbContext _context;
        public TournamentService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Tournament>> GetAllAsync() =>
            await _context.Tournaments.OrderByDescending(t => t.StartDate).ToListAsync();

        public async Task<Tournament> GetByIdAsync(int id) =>
            await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Tournament> GetDetailsAsync(int id) =>
            await _context.Tournaments
                .Include(t => t.Matches).ThenInclude(m => m.TeamA)
                .Include(t => t.Matches).ThenInclude(m => m.TeamB)
                .Include(t => t.Registrations).ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<int> CreateAsync(Tournament tournament)
        {
            tournament.CreatedOn = DateTime.UtcNow;
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();
            return tournament.Id;
        }

        public async Task UpdateAsync(Tournament tournament)
        {
            _context.Tournaments.Update(tournament);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var t = await _context.Tournaments.FindAsync(id);
            if (t != null)
            {
                _context.Tournaments.Remove(t);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Tournament>> GetUpcomingAsync(int count) =>
            await _context.Tournaments
                .Where(t => t.StartDate >= DateTime.UtcNow.Date)
                .OrderBy(t => t.StartDate)
                .Take(count)
                .ToListAsync();

        public async Task RegisterTeamAsync(int tournamentId, int teamId)
        {
            bool exists = await _context.TournamentRegistrations
                .AnyAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);
            if (exists) return;

            _context.TournamentRegistrations.Add(new TournamentRegistration
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                IsApproved = false
            });
            await _context.SaveChangesAsync();
        }
    }
}
