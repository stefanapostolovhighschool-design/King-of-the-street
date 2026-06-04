using KingOfTheStreet.Data.Models;

namespace KingOfTheStreet.Services.Interfaces
{
   
    public interface ILineupAdvisor
    {
        Task<IEnumerable<Player>> SuggestBestLineupAsync(int teamId);
        Task<int?> PredictTournamentWinnerAsync(int tournamentId);
    }
}
