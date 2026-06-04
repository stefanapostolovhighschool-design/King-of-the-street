namespace KingOfTheStreet.Services.Interfaces
{
    public interface IMvpVoteService
    {
        Task<bool> VoteAsync(string userId, int tournamentId, int playerId);
        Task<int> GetVoteCountAsync(int playerId, int tournamentId);
    }
}
