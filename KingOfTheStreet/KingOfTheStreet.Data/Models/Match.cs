using System.ComponentModel.DataAnnotations;

namespace KingOfTheStreet.Data.Models
{
    public enum MatchStatus
    {
        Scheduled = 0,
        Live = 1,
        Finished = 2
    }

    public class Match
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int? TeamAId { get; set; }
        public Team TeamA { get; set; }

        public int? TeamBId { get; set; }
        public Team TeamB { get; set; }

        public int ScoreA { get; set; }
        public int ScoreB { get; set; }

        public int? WinnerId { get; set; }
        public Team Winner { get; set; }

        public int? CourtId { get; set; }
        public Court Court { get; set; }

        public DateTime MatchDate { get; set; }

        [Range(1, 12)]
        public int Round { get; set; }

        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

        public int? MvpPlayerId { get; set; }
        public Player MvpPlayer { get; set; }

        public ICollection<PlayerMatchStat> PlayerStats { get; set; } = new List<PlayerMatchStat>();
    }
}
