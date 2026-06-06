using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Implementations;
using NUnit.Framework;

namespace KingOfTheStreet.Tests
{
    [TestFixture]
    public class TeamServiceTests
    {
        [Test]
        public async Task CreateAsync_AddsTeam_AndReturnsId()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);

            var id = await service.CreateAsync(new Team { Name = "Court Generals", CaptainId = "u1" });

            Assert.That(id, Is.GreaterThan(0));
            Assert.That(ctx.Teams.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task ApproveAsync_SetsIsApprovedTrue()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);
            var id = await service.CreateAsync(new Team { Name = "Ballers", CaptainId = "u1", IsApproved = false });

            await service.ApproveAsync(id);

            var team = await service.GetByIdAsync(id);
            Assert.That(team.IsApproved, Is.True);
        }

        [Test]
        public async Task SearchAsync_FiltersByName()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);
            await service.CreateAsync(new Team { Name = "Downtown Dragons", CaptainId = "u1" });
            await service.CreateAsync(new Team { Name = "Uptown Tigers", CaptainId = "u1" });

            var results = (await service.SearchAsync("Dragon")).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().Name, Is.EqualTo("Downtown Dragons"));
        }

        [Test]
        public async Task SearchAsync_EmptyTerm_ReturnsAll()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);
            await service.CreateAsync(new Team { Name = "A", CaptainId = "u1" });
            await service.CreateAsync(new Team { Name = "B", CaptainId = "u1" });

            var results = (await service.SearchAsync("")).ToList();

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        public void CalculateTeamRating_ReturnsZero_ForEmptyRoster()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);

            var rating = service.CalculateTeamRating(new Team { Name = "Empty" });

            Assert.That(rating, Is.EqualTo(0));
        }

        [Test]
        public void CalculateTeamRating_AveragesTopFivePlayers()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);
            var team = new Team { Name = "Stars" };
           
            foreach (var r in new[] { 90, 80, 70, 60, 50, 40 })
                team.Players.Add(TestHelper.MakePlayer($"P{r}", 0, r));

            var rating = service.CalculateTeamRating(team);

            Assert.That(rating, Is.EqualTo(70).Within(0.01));
        }

        [Test]
        public async Task GetTopTeamsAsync_OrdersByRatingDescending()
        {
            using var ctx = TestHelper.NewContext();
            ctx.Teams.Add(TestHelper.MakeTeam("Weak", 3, 50));
            ctx.Teams.Add(TestHelper.MakeTeam("Strong", 3, 90));
            await ctx.SaveChangesAsync();
            var service = new TeamService(ctx);

            var top = (await service.GetTopTeamsAsync(2)).ToList();

            Assert.That(top.First().Name, Is.EqualTo("Strong"));
        }

        [Test]
        public async Task DeleteAsync_RemovesTeam()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TeamService(ctx);
            var id = await service.CreateAsync(new Team { Name = "Temp", CaptainId = "u1" });

            await service.DeleteAsync(id);

            Assert.That(ctx.Teams.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetDetailsAsync_IncludesPlayers()
        {
            using var ctx = TestHelper.NewContext();
            ctx.Teams.Add(TestHelper.MakeTeam("Loaded", 3, 75));
            await ctx.SaveChangesAsync();
            var service = new TeamService(ctx);
            var teamId = ctx.Teams.First().Id;

            var details = await service.GetDetailsAsync(teamId);

            Assert.That(details.Players.Count, Is.EqualTo(3));
        }
    }
}
