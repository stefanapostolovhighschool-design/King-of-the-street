using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Models;

namespace KingOfTheStreet.Services.Interfaces
{
    public interface IMatchService
    {
        Task<IEnumerable<Match>> GetScheduleAsync(int tournamentId);
        Task<Match> GetByIdAsync(int id);
        Task<IEnumerable<Match>> GetLiveAsync(int tournamentId);
        Task EnterResultAsync(int matchId, int scoreA, int scoreB);
        Task<MatchSimulationResult> SimulateMatchAsync(int matchId, int? seed = null);
        Task<IEnumerable<MatchSimulationResult>> SimulateTournamentAsync(int tournamentId, int? seed = null);
    }
}
