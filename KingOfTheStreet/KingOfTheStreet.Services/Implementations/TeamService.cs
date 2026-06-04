using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        public TeamService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Team>> GetAllAsync() =>
            await _context.Teams.Include(t => t.Players).OrderBy(t => t.Name).ToListAsync();

        public async Task<IEnumerable<Team>> SearchAsync(string term)
        {
            var query = _context.Teams.Include(t => t.Players).AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(t => t.Name.Contains(term));
            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<Team> GetByIdAsync(int id) =>
            await _context.Teams.FindAsync(id);

        public async Task<Team> GetDetailsAsync(int id) =>
            await _context.Teams
                .Include(t => t.Players)
                .Include(t => t.Captain)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<int> CreateAsync(Team team)
        {
            team.CreatedOn = DateTime.UtcNow;
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team.Id;
        }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ApproveAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                team.IsApproved = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Team>> GetTopTeamsAsync(int count)
        {
            var teams = await _context.Teams.Include(t => t.Players).ToListAsync();
            return teams.OrderByDescending(CalculateTeamRating).Take(count);
        }

        public double CalculateTeamRating(Team team)
        {
            if (team?.Players == null || team.Players.Count == 0) return 0;
          
            return Math.Round(
                team.Players
                    .OrderByDescending(p => p.OverallRating)
                    .Take(5)
                    .Average(p => p.OverallRating), 1);
        }
    }
}
