using KingOfTheStreet.Data.Models;

namespace KingOfTheStreet.Services.Interfaces
{
    public interface IPlayerService
    {
        Task<IEnumerable<Player>> GetAllAsync();
        Task<IEnumerable<Player>> SearchAsync(string term);
        Task<Player> GetByIdAsync(int id);
        Task<int> CreateAsync(Player player);
        Task UpdateAsync(Player player);
        Task DeleteAsync(int id);
    }
}
