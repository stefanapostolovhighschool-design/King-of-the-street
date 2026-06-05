using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Models;

namespace KingOfTheStreet.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class HomeViewModel
    {
        public List<Tournament> UpcomingTournaments { get; set; } = new();
        public List<Team> TopTeams { get; set; } = new();
        public List<LeaderRow> TopScorers { get; set; } = new();
        public DashboardStats Stats { get; set; } = new();
    }

    public class LeaderboardViewModel
    {
        public List<LeaderRow> TopScorers { get; set; } = new();
        public List<LeaderRow> TopRebounders { get; set; } = new();
        public List<LeaderRow> TopAssists { get; set; } = new();
        public List<LeaderRow> BestDefenders { get; set; } = new();
    }
}
