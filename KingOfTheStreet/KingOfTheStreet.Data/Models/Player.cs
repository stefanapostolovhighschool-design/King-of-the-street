using System.ComponentModel.DataAnnotations;

namespace KingOfTheStreet.Data.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; }

        [Range(14, 60)]
        public int Age { get; set; }

        [Range(140, 240)]
        public int Height { get; set; } 

        [Required]
        [StringLength(40)]
        public string Position { get; set; }

        public int? TeamId { get; set; }
        public Team Team { get; set; }

        
        [Range(0, 100)] public int ThreePointPercentage { get; set; }
        [Range(0, 100)] public int MidRangePercentage { get; set; }
        [Range(0, 100)] public int FreeThrowPercentage { get; set; }
        [Range(0, 100)] public int PaintScoringPercentage { get; set; }

        
        [Range(0, 100)] public int DefenseRating { get; set; }
        [Range(0, 100)] public int ReboundingRating { get; set; }
        [Range(0, 100)] public int PassingRating { get; set; }
        [Range(0, 100)] public int SpeedRating { get; set; }
        [Range(0, 100)] public int StaminaRating { get; set; }

        public ICollection<PlayerMatchStat> MatchStats { get; set; } = new List<PlayerMatchStat>();

       
        public double OverallRating =>
            (ThreePointPercentage + MidRangePercentage + FreeThrowPercentage + PaintScoringPercentage
             + DefenseRating + ReboundingRating + PassingRating + SpeedRating + StaminaRating) / 9.0;
    }
}
