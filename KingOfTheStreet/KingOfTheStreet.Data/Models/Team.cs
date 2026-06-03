using System.ComponentModel.DataAnnotations;

namespace KingOfTheStreet.Data.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(400)]
        public string LogoUrl { get; set; }

        public string CaptainId { get; set; }
        public ApplicationUser Captain { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<TournamentRegistration> Registrations { get; set; } = new List<TournamentRegistration>();
    }
}
