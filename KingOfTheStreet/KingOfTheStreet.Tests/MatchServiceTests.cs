using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Implementations;
using KingOfTheStreet.Services.Simulation;
using NUnit.Framework;

namespace KingOfTheStreet.Tests
{
    [TestFixture]
    public class MatchServiceTests
    {
        private async Task<(KingOfTheStreet.Data.ApplicationDbContext ctx, int matchId)> SetupMatchAsync()
        {
            var ctx = TestHelper.NewContext();
            var (tournamentId, teamAId, teamBId) = await TestHelper.SeedTwoTeamsAndTournamentAsync(ctx);

            var match = new Match
            {
                TournamentId = tournamentId,
                TeamAId = teamAId,
                TeamBId = teamBId,
                Round = 1,
                MatchDate = DateTime.UtcNow,
                Status = MatchStatus.Scheduled
            };
            ctx.Matches.Add(match);
            await ctx.SaveChangesAsync();
            return (ctx, match.Id);
        }

        [Test]
        public async Task EnterResultAsync_SetsScoresAndWinner()
        {
            var (ctx, matchId) = await SetupMatchAsync();
            var service = new MatchService(ctx, new MatchSimulator());

            await service.EnterResultAsync(matchId, 21, 15);

            var match = await service.GetByIdAsync(matchId);
            Assert.That(match.ScoreA, Is.EqualTo(21));
            Assert.That(match.ScoreB, Is.EqualTo(15));
            Assert.That(match.Status, Is.EqualTo(MatchStatus.Finished));
            Assert.That(match.WinnerId, Is.EqualTo(match.TeamAId));
            ctx.Dispose();
        }

        [Test]
        public async Task SimulateMatchAsync_PersistsScoresAndBoxScores()
        {
            var (ctx, matchId) = await SetupMatchAsync();
            var service = new MatchService(ctx, new MatchSimulator());

            var result = await service.SimulateMatchAsync(matchId, seed: 42);

            var match = await service.GetByIdAsync(matchId);
            Assert.That(match.Status, Is.EqualTo(MatchStatus.Finished));
            Assert.That(match.ScoreA, Is.EqualTo(result.ScoreA));
            Assert.That(match.ScoreB, Is.EqualTo(result.ScoreB));
            Assert.That(ctx.PlayerMatchStats.Any(), Is.True);
            ctx.Dispose();
        }

        [Test]
        public async Task SimulateMatchAsync_IsIdempotent_DoesNotDuplicateStats()
        {
            var (ctx, matchId) = await SetupMatchAsync();
            var service = new MatchService(ctx, new MatchSimulator());

            await service.SimulateMatchAsync(matchId, seed: 42);
            int firstCount = ctx.PlayerMatchStats.Count(s => s.MatchId == matchId);
            await service.SimulateMatchAsync(matchId, seed: 42);
            int secondCount = ctx.PlayerMatchStats.Count(s => s.MatchId == matchId);

            Assert.That(secondCount, Is.EqualTo(firstCount));
            ctx.Dispose();
        }

        [Test]
        public async Task SimulateMatchAsync_AssignsMvp()
        {
            var (ctx, matchId) = await SetupMatchAsync();
            var service = new MatchService(ctx, new MatchSimulator());

            await service.SimulateMatchAsync(matchId, seed: 5);

            var match = await service.GetByIdAsync(matchId);
            Assert.That(match.MvpPlayerId, Is.Not.Null);
            ctx.Dispose();
        }

        [Test]
        public async Task GetScheduleAsync_ReturnsMatchesForTournament()
        {
            var (ctx, matchId) = await SetupMatchAsync();
            var service = new MatchService(ctx, new MatchSimulator());
            var match = await service.GetByIdAsync(matchId);

            var schedule = (await service.GetScheduleAsync(match.TournamentId)).ToList();

            Assert.That(schedule.Count, Is.EqualTo(1));
            ctx.Dispose();
        }

        [Test]
        public async Task GetLiveAsync_ReturnsFinishedMatches()
        {
            var (ctx, matchId) = await SetupMatchAsync();
            var service = new MatchService(ctx, new MatchSimulator());
            await service.EnterResultAsync(matchId, 21, 10);
            var match = await service.GetByIdAsync(matchId);

            var live = (await service.GetLiveAsync(match.TournamentId)).ToList();

            Assert.That(live.Count, Is.EqualTo(1));
            ctx.Dispose();
        }
    }
}
