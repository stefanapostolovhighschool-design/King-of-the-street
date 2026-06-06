using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Implementations;
using NUnit.Framework;

namespace KingOfTheStreet.Tests
{
    [TestFixture]
    public class PlayerServiceTests
    {
        [Test]
        public async Task CreateAsync_AddsPlayer()
        {
            using var ctx = TestHelper.NewContext();
            var service = new PlayerService(ctx);

            var id = await service.CreateAsync(TestHelper.MakePlayer("Mike Jordan", 0, 95));

            Assert.That(id, Is.GreaterThan(0));
            Assert.That(ctx.Players.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_MatchesNameOrPosition()
        {
            using var ctx = TestHelper.NewContext();
            var service = new PlayerService(ctx);
            var p = TestHelper.MakePlayer("Steph Curry", 0, 90);
            p.Position = "Point Guard";
            await service.CreateAsync(p);
            await service.CreateAsync(TestHelper.MakePlayer("Big Man", 0, 80));

            var byName = (await service.SearchAsync("Curry")).ToList();
            var byPosition = (await service.SearchAsync("Point")).ToList();

            Assert.That(byName.Count, Is.EqualTo(1));
            Assert.That(byPosition.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateAsync_PersistsChanges()
        {
            using var ctx = TestHelper.NewContext();
            var service = new PlayerService(ctx);
            var id = await service.CreateAsync(TestHelper.MakePlayer("Old Name", 0, 70));

            var player = await service.GetByIdAsync(id);
            player.FullName = "New Name";
            await service.UpdateAsync(player);

            Assert.That((await service.GetByIdAsync(id)).FullName, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task DeleteAsync_RemovesPlayer()
        {
            using var ctx = TestHelper.NewContext();
            var service = new PlayerService(ctx);
            var id = await service.CreateAsync(TestHelper.MakePlayer("Temp", 0, 60));

            await service.DeleteAsync(id);

            Assert.That(ctx.Players.Count(), Is.EqualTo(0));
        }

        [Test]
        public void OverallRating_IsAverageOfNineAttributes()
        {
            var p = TestHelper.MakePlayer("Avg", 0, 80); 
            Assert.That(p.OverallRating, Is.EqualTo(80).Within(0.01));
        }
    }

    [TestFixture]
    public class MvpVoteServiceTests
    {
        [Test]
        public async Task VoteAsync_RecordsVote_FirstTime()
        {
            using var ctx = TestHelper.NewContext();
            var service = new MvpVoteService(ctx);

            var ok = await service.VoteAsync("user1", tournamentId: 1, playerId: 5);

            Assert.That(ok, Is.True);
            Assert.That(ctx.MVPVotes.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task VoteAsync_RejectsSecondVote_SameUserSameTournament()
        {
            using var ctx = TestHelper.NewContext();
            var service = new MvpVoteService(ctx);
            await service.VoteAsync("user1", 1, 5);

            var second = await service.VoteAsync("user1", 1, 7);

            Assert.That(second, Is.False);
            Assert.That(ctx.MVPVotes.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetVoteCountAsync_CountsVotesForPlayer()
        {
            using var ctx = TestHelper.NewContext();
            var service = new MvpVoteService(ctx);
            await service.VoteAsync("user1", 1, 5);
            await service.VoteAsync("user2", 1, 5);

            var count = await service.GetVoteCountAsync(5, 1);

            Assert.That(count, Is.EqualTo(2));
        }
    }

    [TestFixture]
    public class StatisticsServiceTests
    {
        [Test]
        public async Task GetDashboardStatsAsync_CountsEntities()
        {
            using var ctx = TestHelper.NewContext();
            await TestHelper.SeedTwoTeamsAndTournamentAsync(ctx);
            var service = new StatisticsService(ctx);

            var stats = await service.GetDashboardStatsAsync();

            Assert.That(stats.TotalTournaments, Is.EqualTo(1));
            Assert.That(stats.TotalTeams, Is.EqualTo(2));
            Assert.That(stats.TotalPlayers, Is.EqualTo(6));
        }

        [Test]
        public async Task GetTopScorersAsync_RanksByPoints()
        {
            using var ctx = TestHelper.NewContext();
            ctx.Players.Add(TestHelper.MakePlayer("Scorer", 0, 90));
            ctx.Players.Add(TestHelper.MakePlayer("Role", 0, 60));
            await ctx.SaveChangesAsync();
            var p1 = ctx.Players.First().Id;
            var p2 = ctx.Players.Skip(1).First().Id;

            ctx.Matches.Add(new Match { Round = 1, MatchDate = DateTime.UtcNow, Status = MatchStatus.Finished });
            await ctx.SaveChangesAsync();
            var matchId = ctx.Matches.First().Id;

            ctx.PlayerMatchStats.Add(new PlayerMatchStat { MatchId = matchId, PlayerId = p1, TeamId = 1, Points = 25 });
            ctx.PlayerMatchStats.Add(new PlayerMatchStat { MatchId = matchId, PlayerId = p2, TeamId = 1, Points = 8 });
            await ctx.SaveChangesAsync();

            var service = new StatisticsService(ctx);
            var leaders = (await service.GetTopScorersAsync(10)).ToList();

            Assert.That(leaders.First().PlayerId, Is.EqualTo(p1));
            Assert.That(leaders.First().Value, Is.EqualTo(25));
        }

        [Test]
        public async Task GetBestDefendersAsync_CombinesStealsAndBlocks()
        {
            using var ctx = TestHelper.NewContext();
            ctx.Players.Add(TestHelper.MakePlayer("Defender", 0, 85));
            await ctx.SaveChangesAsync();
            var pid = ctx.Players.First().Id;
            ctx.Matches.Add(new Match { Round = 1, MatchDate = DateTime.UtcNow, Status = MatchStatus.Finished });
            await ctx.SaveChangesAsync();
            var matchId = ctx.Matches.First().Id;
            ctx.PlayerMatchStats.Add(new PlayerMatchStat { MatchId = matchId, PlayerId = pid, TeamId = 1, Steals = 3, Blocks = 2 });
            await ctx.SaveChangesAsync();

            var service = new StatisticsService(ctx);
            var leaders = (await service.GetBestDefendersAsync(10)).ToList();

            Assert.That(leaders.First().Value, Is.EqualTo(5));
        }
    }
}
