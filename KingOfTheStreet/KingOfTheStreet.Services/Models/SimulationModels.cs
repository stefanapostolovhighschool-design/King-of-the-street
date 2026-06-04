namespace KingOfTheStreet.Services.Models
{
    public class PlayerBoxScore
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int TeamId { get; set; }
        public int Points { get; set; }
        public int Assists { get; set; }
        public int Rebounds { get; set; }
        public int Steals { get; set; }
        public int Blocks { get; set; }
        public int FieldGoalsMade { get; set; }
        public int FieldGoalsAttempted { get; set; }
        public int ThreePointersMade { get; set; }
        public int ThreePointersAttempted { get; set; }
        public int FreeThrowsMade { get; set; }
        public int FreeThrowsAttempted { get; set; }

        public double FieldGoalPercentage =>
            FieldGoalsAttempted == 0 ? 0 : Math.Round(100.0 * FieldGoalsMade / FieldGoalsAttempted, 1);

        
        public double PerformanceIndex =>
            Points + 1.2 * Rebounds + 1.5 * Assists + 2 * Steals + 2 * Blocks;
    }

    public class MatchSimulationResult
    {
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }
        public string TeamAName { get; set; }
        public string TeamBName { get; set; }
        public int ScoreA { get; set; }
        public int ScoreB { get; set; }
        public int WinnerTeamId { get; set; }
        public string WinnerTeamName { get; set; }
        public int MvpPlayerId { get; set; }
        public string MvpPlayerName { get; set; }
        public List<PlayerBoxScore> BoxScores { get; set; } = new();
        public List<string> PlayByPlay { get; set; } = new();
    }
}
