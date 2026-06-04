using KingOfTheStreet.Data.Models;

namespace KingOfTheStreet.Services.Interfaces
{
    public interface IBracketService
    {
      
        Task GenerateBracketAsync(int tournamentId, int? seed = null);

        
        Task<int?> AdvanceRoundAsync(int tournamentId);

        Task<IEnumerable<Match>> GetBracketAsync(int tournamentId);
    }
}
