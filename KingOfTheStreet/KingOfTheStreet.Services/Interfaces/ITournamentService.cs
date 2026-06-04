using KingOfTheStreet.Data.Models;

namespace KingOfTheStreet.Services.Interfaces
{
    public interface ITournamentService
    {
        Task<IEnumerable<Tournament>> GetAllAsync();
        Task<Tournament> GetByIdAsync(int id);
        Task<Tournament> GetDetailsAsync(int id);
        Task<int> CreateAsync(Tournament tournament);
        Task UpdateAsync(Tournament tournament);
        Task DeleteAsync(int id);
        Task<IEnumerable<Tournament>> GetUpcomingAsync(int count);
        Task RegisterTeamAsync(int tournamentId, int teamId);
    }
}
