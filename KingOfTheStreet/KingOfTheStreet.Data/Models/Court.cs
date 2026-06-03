using System.ComponentModel.DataAnnotations;

namespace KingOfTheStreet.Data.Models
{
    public class Court
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; }

        [Required]
        [StringLength(160)]
        public string Location { get; set; }

        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
