using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Models;

namespace KingOfTheStreet.Services.Simulation
{
    public interface IMatchSimulator
    {
        
        MatchSimulationResult Simulate(Team teamA, Team teamB, int? seed = null);
    }
}
