using KingOfTheStreet.Services.Simulation;
using NUnit.Framework;

namespace KingOfTheStreet.Tests
{
    [TestFixture]
    public class MatchSimulatorTests
    {
        private readonly MatchSimulator _sim = new();

        [Test]
        public void Simulate_IsDeterministic_WithSameSeed()
        {
            var teamA = TestHelper.MakeTeam("Alpha", 3, 80); teamA.Id = 1;
            var teamB = TestHelper.MakeTeam("Beta", 3, 70); teamB.Id = 2;
            foreach (var p in teamA.Players) p.TeamId = 1;
            foreach (var p in teamB.Players) p.TeamId = 2;

            var r1 = _sim.Simulate(teamA, teamB, seed: 42);
            var r2 = _sim.Simulate(teamA, teamB, seed: 42);

            Assert.That(r1.ScoreA, Is.EqualTo(r2.ScoreA));
            Assert.That(r1.ScoreB, Is.EqualTo(r2.ScoreB));
            Assert.That(r1.WinnerTeamId, Is.EqualTo(r2.WinnerTeamId));
            Assert.That(r1.MvpPlayerId, Is.EqualTo(r2.MvpPlayerId));
        }

        [Test]
        public void Simulate_ProducesAWinner_WithinStreetRules()
        {
            var teamA = TestHelper.MakeTeam("Alpha", 3, 80); teamA.Id = 1;
            var teamB = TestHelper.MakeTeam("Beta", 3, 70); teamB.Id = 2;

            var result = _sim.Simulate(teamA, teamB, seed: 7);

           
            Assert.That(result.WinnerTeamId, Is.AnyOf(teamA.Id, teamB.Id));
            
            int winningScore = Math.Max(result.ScoreA, result.ScoreB);
            Assert.That(winningScore, Is.GreaterThanOrEqualTo(21));
            Assert.That(winningScore, Is.LessThanOrEqualTo(29));
        }

        [Test]
        public void Simulate_WinnerHasHigherOrEqualScore()
        {
            var teamA = TestHelper.MakeTeam("Alpha", 3, 85); teamA.Id = 1;
            var teamB = TestHelper.MakeTeam("Beta", 3, 60); teamB.Id = 2;

            var result = _sim.Simulate(teamA, teamB, seed: 3);

            if (result.WinnerTeamId == teamA.Id)
                Assert.That(result.ScoreA, Is.GreaterThanOrEqualTo(result.ScoreB));
            else
                Assert.That(result.ScoreB, Is.GreaterThanOrEqualTo(result.ScoreA));
        }

        [Test]
        public void Simulate_GeneratesBoxScores_ForAllStarters()
        {
            var teamA = TestHelper.MakeTeam("Alpha", 3, 80); teamA.Id = 1;
            var teamB = TestHelper.MakeTeam("Beta", 3, 70); teamB.Id = 2;

            var result = _sim.Simulate(teamA, teamB, seed: 11);

            
            Assert.That(result.BoxScores.Count, Is.EqualTo(6));
        }

        [Test]
        public void Simulate_AssignsMvp()
        {
            var teamA = TestHelper.MakeTeam("Alpha", 3, 80); teamA.Id = 1;
            var teamB = TestHelper.MakeTeam("Beta", 3, 70); teamB.Id = 2;

            var result = _sim.Simulate(teamA, teamB, seed: 99);

            Assert.That(result.MvpPlayerId, Is.GreaterThan(0));
            Assert.That(result.MvpPlayerName, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Simulate_Throws_WhenTeamHasNoPlayers()
        {
            var teamA = TestHelper.MakeTeam("Alpha", 0); teamA.Id = 1;
            var teamB = TestHelper.MakeTeam("Beta", 3); teamB.Id = 2;

            Assert.Throws<InvalidOperationException>(() => _sim.Simulate(teamA, teamB, seed: 1));
        }

        [Test]
        public void Simulate_Throws_OnNullTeam()
        {
            var teamB = TestHelper.MakeTeam("Beta", 3); teamB.Id = 2;

            Assert.Throws<ArgumentNullException>(() => _sim.Simulate(null, teamB, seed: 1));
        }
    }
}
