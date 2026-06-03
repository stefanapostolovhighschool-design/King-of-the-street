using Microsoft.AspNetCore.Identity;

namespace KingOfTheStreet.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(60)]
        public string FirstName { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(60)]
        public string LastName { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public ICollection<Team> CaptainedTeams { get; set; } = new List<Team>();
        public ICollection<MVPVote> MVPVotes { get; set; } = new List<MVPVote>();
    }
}
