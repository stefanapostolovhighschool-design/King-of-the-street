namespace KingOfTheStreet.Services.Interfaces
{
    public interface IAdminService
    {
        Task<int> GetUserCountAsync();
        Task ApproveTeamAsync(int teamId);
        Task SimulateSeasonAsync(int? seed = null);
    }
}
