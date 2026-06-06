using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Implementations;
using NUnit.Framework;

namespace KingOfTheStreet.Tests
{
    [TestFixture]
    public class BracketServiceTests
    {
        private async Task<(KingOfTheStreet.Data.ApplicationDbContext ctx, int tournamentId)> SeedApprovedTeamsAsync(int teamCount)
        {
            var ctx = TestHelper.NewContext();
            var tournament = new Tournament
            {
                Name = "Bracket Cup",
                Description = "desc",
                Location = "loc",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(1),
                MaxTeams = 16
            };
            ctx.Tournaments.Add(tournament);
            for (int i = 0; i < teamCount; i++)
                ctx.Teams.Add(TestHelper.MakeTeam($"Team {i + 1}", 3, 70, approved: true));
            ctx.Courts.Add(new Court { Name = "Court 1", Location = "X" });
            await ctx.SaveChangesAsync();
            return (ctx, tournament.Id);
        }

        [Test]
        public async Task GenerateBracketAsync_CreatesRoundOneMatches_ForEvenTeams()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(4);
            var service = new BracketService(ctx);

            await service.GenerateBracketAsync(tournamentId, seed: 1);

            
            Assert.That(ctx.Matches.Count(m => m.TournamentId == tournamentId), Is.EqualTo(2));
        }

        [Test]
        public async Task GenerateBracketAsync_HandlesByes_ForOddTeams()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(3);
            var service = new BracketService(ctx);

            await service.GenerateBracketAsync(tournamentId, seed: 1);

            
            var matches = ctx.Matches.Where(m => m.TournamentId == tournamentId).ToList();
            Assert.That(matches.Count, Is.EqualTo(2));
            Assert.That(matches.Any(m => m.TeamBId == null && m.Status == MatchStatus.Finished), Is.True);
        }

        [Test]
        public async Task GenerateBracketAsync_SetsTournamentInProgress()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(4);
            var service = new BracketService(ctx);

            await service.GenerateBracketAsync(tournamentId, seed: 1);

            var t = await ctx.Tournaments.FindAsync(tournamentId);
            Assert.That(t.Status, Is.EqualTo(TournamentStatus.InProgress));
        }

        [Test]
        public async Task GenerateBracketAsync_Throws_WhenFewerThanTwoTeams()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(1);
            var service = new BracketService(ctx);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.GenerateBracketAsync(tournamentId, seed: 1));
        }

        [Test]
        public async Task GenerateBracketAsync_Throws_WhenTournamentMissing()
        {
            var ctx = TestHelper.NewContext();
            var service = new BracketService(ctx);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.GenerateBracketAsync(12345, seed: 1));
        }

        [Test]
        public async Task GenerateBracketAsync_IsDeterministic_WithSeed()
        {
            var (ctx1, t1) = await SeedApprovedTeamsAsync(4);
            var (ctx2, t2) = await SeedApprovedTeamsAsync(4);

            await new BracketService(ctx1).GenerateBracketAsync(t1, seed: 123);
            await new BracketService(ctx2).GenerateBracketAsync(t2, seed: 123);

            var pairs1 = ctx1.Matches.OrderBy(m => m.Id).Select(m => (m.TeamAId, m.TeamBId)).ToList();
            var pairs2 = ctx2.Matches.OrderBy(m => m.Id).Select(m => (m.TeamAId, m.TeamBId)).ToList();

            
            Assert.That(pairs1.Count, Is.EqualTo(pairs2.Count));
        }

        [Test]
        public async Task AdvanceRoundAsync_Throws_WhenRoundUnfinished()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(4);
            var service = new BracketService(ctx);
            await service.GenerateBracketAsync(tournamentId, seed: 1);

            
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.AdvanceRoundAsync(tournamentId));
        }

        [Test]
        public async Task AdvanceRoundAsync_CreatesNextRound_WhenRoundFinished()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(4);
            var service = new BracketService(ctx);
            await service.GenerateBracketAsync(tournamentId, seed: 1);

            
            foreach (var m in ctx.Matches.Where(m => m.TournamentId == tournamentId))
            {
                m.Status = MatchStatus.Finished;
                m.WinnerId = m.TeamAId;
            }
            await ctx.SaveChangesAsync();

            var champion = await service.AdvanceRoundAsync(tournamentId);

            
            Assert.That(champion, Is.Null);
            Assert.That(ctx.Matches.Count(m => m.TournamentId == tournamentId && m.Round == 2), Is.EqualTo(1));
        }

        [Test]
        public async Task GetBracketAsync_ReturnsMatchesOrderedByRound()
        {
            var (ctx, tournamentId) = await SeedApprovedTeamsAsync(4);
            var service = new BracketService(ctx);
            await service.GenerateBracketAsync(tournamentId, seed: 1);

            var bracket = (await service.GetBracketAsync(tournamentId)).ToList();

            Assert.That(bracket.Count, Is.EqualTo(2));
            Assert.That(bracket.All(m => m.Round == 1), Is.True);
        }
    }
}
