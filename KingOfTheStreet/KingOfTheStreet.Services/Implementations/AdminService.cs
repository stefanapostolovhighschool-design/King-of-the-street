using KingOfTheStreet.Data;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMatchService _matchService;

        public AdminService(ApplicationDbContext context, IMatchService matchService)
        {
            _context = context;
            _matchService = matchService;
        }

        public async Task<int> GetUserCountAsync() => await _context.Users.CountAsync();

        public async Task ApproveTeamAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team != null)
            {
                team.IsApproved = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SimulateSeasonAsync(int? seed = null)
        {
            var tournamentIds = await _context.Tournaments.Select(t => t.Id).ToListAsync();
            int offset = 0;
            foreach (var id in tournamentIds)
            {
                int? s = seed.HasValue ? seed.Value + offset++ : (int?)null;
                await _matchService.SimulateTournamentAsync(id, s);
            }
        }
    }
}
