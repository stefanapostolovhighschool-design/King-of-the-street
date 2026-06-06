using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Implementations;
using NUnit.Framework;

namespace KingOfTheStreet.Tests
{
    [TestFixture]
    public class TournamentServiceTests
    {
        private Tournament SampleTournament(string name = "Summer Slam") => new Tournament
        {
            Name = name,
            Description = "A street tournament",
            Location = "Rucker Park",
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(3),
            MaxTeams = 8
        };

        [Test]
        public async Task CreateAsync_AddsTournament_AndReturnsId()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);

            var id = await service.CreateAsync(SampleTournament());

            Assert.That(id, Is.GreaterThan(0));
            Assert.That(ctx.Tournaments.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsCorrectTournament()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);
            var id = await service.CreateAsync(SampleTournament("Winter Classic"));

            var found = await service.GetByIdAsync(id);

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Name, Is.EqualTo("Winter Classic"));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenMissing()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);

            var found = await service.GetByIdAsync(999);

            Assert.That(found, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAll_OrderedByStartDateDescending()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);
            var early = SampleTournament("Early"); early.StartDate = DateTime.UtcNow.Date.AddDays(1);
            var late = SampleTournament("Late"); late.StartDate = DateTime.UtcNow.Date.AddDays(10);
            await service.CreateAsync(early);
            await service.CreateAsync(late);

            var all = (await service.GetAllAsync()).ToList();

            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.First().Name, Is.EqualTo("Late"));
        }

        [Test]
        public async Task UpdateAsync_PersistsChanges()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);
            var id = await service.CreateAsync(SampleTournament());

            var t = await service.GetByIdAsync(id);
            t.Name = "Renamed Cup";
            await service.UpdateAsync(t);

            var reloaded = await service.GetByIdAsync(id);
            Assert.That(reloaded.Name, Is.EqualTo("Renamed Cup"));
        }

        [Test]
        public async Task DeleteAsync_RemovesTournament()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);
            var id = await service.CreateAsync(SampleTournament());

            await service.DeleteAsync(id);

            Assert.That(ctx.Tournaments.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task RegisterTeamAsync_AddsRegistration()
        {
            using var ctx = TestHelper.NewContext();
            var (tournamentId, teamAId, _) = await TestHelper.SeedTwoTeamsAndTournamentAsync(ctx);
            var service = new TournamentService(ctx);

            await service.RegisterTeamAsync(tournamentId, teamAId);

            Assert.That(ctx.TournamentRegistrations.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task RegisterTeamAsync_IsIdempotent_NoDuplicateRegistration()
        {
            using var ctx = TestHelper.NewContext();
            var (tournamentId, teamAId, _) = await TestHelper.SeedTwoTeamsAndTournamentAsync(ctx);
            var service = new TournamentService(ctx);

            await service.RegisterTeamAsync(tournamentId, teamAId);
            await service.RegisterTeamAsync(tournamentId, teamAId);

            Assert.That(ctx.TournamentRegistrations.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetUpcomingAsync_ReturnsOnlyFutureTournaments()
        {
            using var ctx = TestHelper.NewContext();
            var service = new TournamentService(ctx);
            var past = SampleTournament("Past"); past.StartDate = DateTime.UtcNow.Date.AddDays(-5);
            var future = SampleTournament("Future"); future.StartDate = DateTime.UtcNow.Date.AddDays(5);
            await service.CreateAsync(past);
            await service.CreateAsync(future);

            var upcoming = (await service.GetUpcomingAsync(10)).ToList();

            Assert.That(upcoming.Count, Is.EqualTo(1));
            Assert.That(upcoming.First().Name, Is.EqualTo("Future"));
        }
    }
}
