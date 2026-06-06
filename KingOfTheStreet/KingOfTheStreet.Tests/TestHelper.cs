using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Tests
{
  
    public static class TestHelper
    {
        public static ApplicationDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
            return new ApplicationDbContext(options);
        }

        public static Player MakePlayer(string name, int teamId, int baseRating = 70)
        {
            return new Player
            {
                FullName = name,
                Age = 25,
                Height = 190,
                Position = "Guard",
                TeamId = teamId,
                ThreePointPercentage = baseRating,
                MidRangePercentage = baseRating,
                FreeThrowPercentage = baseRating,
                PaintScoringPercentage = baseRating,
                DefenseRating = baseRating,
                ReboundingRating = baseRating,
                PassingRating = baseRating,
                SpeedRating = baseRating,
                StaminaRating = baseRating
            };
        }

        public static Team MakeTeam(string name, int playerCount = 3, int baseRating = 70, bool approved = true)
        {
            var team = new Team { Name = name, IsApproved = approved, CaptainId = "seed-captain" };
            for (int i = 0; i < playerCount; i++)
                team.Players.Add(MakePlayer($"{name} Player {i + 1}", 0, baseRating + i));
            return team;
        }

        
        public static async Task<(int tournamentId, int teamAId, int teamBId)> SeedTwoTeamsAndTournamentAsync(ApplicationDbContext ctx)
        {
            var teamA = MakeTeam("Court Kings", 3, 80);
            var teamB = MakeTeam("Street Ballers", 3, 70);
            ctx.Teams.AddRange(teamA, teamB);

            var tournament = new Tournament
            {
                Name = "Test Slam",
                Description = "Test tournament",
                Location = "Test Court",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                MaxTeams = 8,
                Status = TournamentStatus.RegistrationOpen
            };
            ctx.Tournaments.Add(tournament);
            ctx.Courts.Add(new Court { Name = "Center Court", Location = "Test City" });
            await ctx.SaveChangesAsync();

            return (tournament.Id, teamA.Id, teamB.Id);
        }
    }
}
