using KingOfTheStreet.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KingOfTheStreet.Data
{
    public static class DbSeeder
    {
        public const string AdminRole = "Administrator";
        public const string UserRole = "User";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            
            if ((await context.Database.GetPendingMigrationsAsync()).Any()
                || (await context.Database.GetAppliedMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

            
            foreach (var role in new[] { AdminRole, UserRole })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            
            const string adminEmail = "admin@kingofthestreet.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    RegistrationDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, AdminRole);
            }

            
            const string userEmail = "captain@kingofthestreet.com";
            ApplicationUser captain = await userManager.FindByEmailAsync(userEmail);
            if (captain == null)
            {
                captain = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    FirstName = "Marcus",
                    LastName = "Court",
                    RegistrationDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(captain, "Captain123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(captain, UserRole);
            }

            
            if (!await context.Courts.AnyAsync())
            {
                context.Courts.AddRange(
                    new Court { Name = "Rucker Park", Location = "Harlem, New York" },
                    new Court { Name = "Venice Beach Courts", Location = "Los Angeles, California" }
                );
                await context.SaveChangesAsync();
            }

            
            if (!await context.Teams.AnyAsync())
            {
                var teams = new List<Team>
                {
                    new Team { Name = "Street Kings", LogoUrl = "/images/team1.png", CaptainId = captain.Id, IsApproved = true },
                    new Team { Name = "Concrete Crushers", LogoUrl = "/images/team2.png", CaptainId = captain.Id, IsApproved = true },
                    new Team { Name = "Asphalt Assassins", LogoUrl = "/images/team3.png", CaptainId = captain.Id, IsApproved = true },
                    new Team { Name = "Downtown Ballers", LogoUrl = "/images/team4.png", CaptainId = captain.Id, IsApproved = true },
                    new Team { Name = "Court Generals", LogoUrl = "/images/team5.png", CaptainId = captain.Id, IsApproved = false }
                };
                context.Teams.AddRange(teams);
                await context.SaveChangesAsync();

                var rnd = new Random(42);
                string[] positions = { "Guard", "Forward", "Center" };
                string[] names =
                {
                    "Tyrone Banks","DeShawn Miles","Carlos Vega","Andre Knight","Jamal Pierce",
                    "Eddie Stone","Malik Rivera","Chris Holloway","Devin Park","Omar Sayed",
                    "Trey Wilson","Bobby Chen","Luis Romero","Kevin Daniels","Nate Brooks"
                };

                int n = 0;
                foreach (var team in teams)
                {
                    int playersForTeam = (team == teams[4]) ? 3 : 3; 
                    for (int i = 0; i < playersForTeam && n < names.Length; i++, n++)
                    {
                        context.Players.Add(new Player
                        {
                            FullName = names[n],
                            Age = rnd.Next(18, 33),
                            Height = rnd.Next(178, 210),
                            Position = positions[i % positions.Length],
                            TeamId = team.Id,
                            ThreePointPercentage = rnd.Next(28, 45),
                            MidRangePercentage = rnd.Next(40, 78),
                            FreeThrowPercentage = rnd.Next(60, 92),
                            PaintScoringPercentage = rnd.Next(55, 90),
                            DefenseRating = rnd.Next(60, 95),
                            ReboundingRating = rnd.Next(55, 92),
                            PassingRating = rnd.Next(55, 96),
                            SpeedRating = rnd.Next(60, 95),
                            StaminaRating = rnd.Next(65, 95)
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            
            if (!await context.Tournaments.AnyAsync())
            {
                context.Tournaments.AddRange(
                    new Tournament
                    {
                        Name = "Summer Street Slam",
                        Description = "The premier 3v3 summer tournament on the East Coast.",
                        Location = "Harlem, New York",
                        StartDate = DateTime.UtcNow.AddDays(7),
                        EndDate = DateTime.UtcNow.AddDays(9),
                        MaxTeams = 8,
                        Status = TournamentStatus.RegistrationOpen
                    },
                    new Tournament
                    {
                        Name = "West Coast Showdown",
                        Description = "Beach hoops championship under the California sun.",
                        Location = "Venice Beach, California",
                        StartDate = DateTime.UtcNow.AddDays(14),
                        EndDate = DateTime.UtcNow.AddDays(16),
                        MaxTeams = 8,
                        Status = TournamentStatus.Upcoming
                    },
                    new Tournament
                    {
                        Name = "Winter Hardcourt Classic",
                        Description = "Indoor 3v3 invitational for the toughest crews.",
                        Location = "Chicago, Illinois",
                        StartDate = DateTime.UtcNow.AddDays(30),
                        EndDate = DateTime.UtcNow.AddDays(32),
                        MaxTeams = 4,
                        Status = TournamentStatus.Upcoming
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
