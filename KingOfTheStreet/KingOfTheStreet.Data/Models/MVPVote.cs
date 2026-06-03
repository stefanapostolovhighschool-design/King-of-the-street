namespace KingOfTheStreet.Data.Models
{
    public class MVPVote
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
