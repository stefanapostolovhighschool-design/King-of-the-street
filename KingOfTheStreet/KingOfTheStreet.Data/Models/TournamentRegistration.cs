namespace KingOfTheStreet.Data.Models
{
    
    public class TournamentRegistration
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public bool IsApproved { get; set; }
        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;
    }
}
