using KingOfTheStreet.Data.Models;

namespace KingOfTheStreet.Services.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<Team>> GetAllAsync();
        Task<IEnumerable<Team>> SearchAsync(string term);
        Task<Team> GetByIdAsync(int id);
        Task<Team> GetDetailsAsync(int id);
        Task<int> CreateAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(int id);
        Task ApproveAsync(int id);
        Task<IEnumerable<Team>> GetTopTeamsAsync(int count);
        double CalculateTeamRating(Team team);
    }
}
