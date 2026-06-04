namespace KingOfTheStreet.Services.Models
{
    public class LeaderRow
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string TeamName { get; set; }
        public double Value { get; set; }
        public int GamesPlayed { get; set; }
    }

    public class DashboardStats
    {
        public int TotalTournaments { get; set; }
        public int TotalTeams { get; set; }
        public int TotalPlayers { get; set; }
        public int UpcomingMatches { get; set; }
        public int TotalUsers { get; set; }
    }
}
