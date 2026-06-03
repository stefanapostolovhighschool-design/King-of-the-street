using System.ComponentModel.DataAnnotations;

namespace KingOfTheStreet.Data.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(160)]
        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Range(2, 64)]
        public int MaxTeams { get; set; }

        public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<TournamentRegistration> Registrations { get; set; } = new List<TournamentRegistration>();
        public ICollection<MVPVote> MVPVotes { get; set; } = new List<MVPVote>();
    }
}
