using KingOfTheStreet.Services.Models;

namespace KingOfTheStreet.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<IEnumerable<LeaderRow>> GetTopScorersAsync(int count);
        Task<IEnumerable<LeaderRow>> GetTopRebloundersAsync(int count);
        Task<IEnumerable<LeaderRow>> GetTopAssistLeadersAsync(int count);
        Task<IEnumerable<LeaderRow>> GetBestDefendersAsync(int count);
        Task<IEnumerable<LeaderRow>> GetMvpRankingAsync(int tournamentId, int count);
        Task<DashboardStats> GetDashboardStatsAsync();
    }
}
