using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;
        public PlayerService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Player>> GetAllAsync() =>
            await _context.Players.Include(p => p.Team).OrderBy(p => p.FullName).ToListAsync();

        public async Task<IEnumerable<Player>> SearchAsync(string term)
        {
            var query = _context.Players.Include(p => p.Team).AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(p => p.FullName.Contains(term) || p.Position.Contains(term));
            return await query.OrderBy(p => p.FullName).ToListAsync();
        }

        public async Task<Player> GetByIdAsync(int id) =>
            await _context.Players.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<int> CreateAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player.Id;
        }

        public async Task UpdateAsync(Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }
    }
}
