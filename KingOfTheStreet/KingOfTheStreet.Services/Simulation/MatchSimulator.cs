using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Models;

namespace KingOfTheStreet.Services.Simulation
{
   
    public class MatchSimulator : IMatchSimulator
    {
      
        private const int TargetScore = 21;
        private const int HardCap = 29;
        private const int MaxPossessions = 200;

        public MatchSimulationResult Simulate(Team teamA, Team teamB, int? seed = null)
        {
            if (teamA == null) throw new ArgumentNullException(nameof(teamA));
            if (teamB == null) throw new ArgumentNullException(nameof(teamB));

            var rnd = seed.HasValue ? new Random(seed.Value) : new Random();

            var rosterA = StartingFive(teamA);
            var rosterB = StartingFive(teamB);

            if (rosterA.Count == 0 || rosterB.Count == 0)
                throw new InvalidOperationException("Both teams must have at least one player to simulate a match.");

            var box = new Dictionary<int, PlayerBoxScore>();
            foreach (var p in rosterA.Concat(rosterB))
            {
                box[p.Id] = new PlayerBoxScore
                {
                    PlayerId = p.Id,
                    PlayerName = p.FullName,
                    TeamId = p.TeamId ?? 0
                };
            }

            int scoreA = 0, scoreB = 0;
            var playByPlay = new List<string>();
            bool teamAHasBall = rnd.Next(2) == 0; // tip-off
            int possessions = 0;

            while (!IsGameOver(scoreA, scoreB) && possessions < MaxPossessions)
            {
                possessions++;
                var offense = teamAHasBall ? rosterA : rosterB;
                var defense = teamAHasBall ? rosterB : rosterA;

                int pointsScored = RunPossession(offense, defense, box, rnd, playByPlay, teamAHasBall);

                if (teamAHasBall) scoreA += pointsScored;
                else scoreB += pointsScored;

                teamAHasBall = !teamAHasBall; 
            }

            int winnerId = scoreA >= scoreB ? teamA.Id : teamB.Id;
            string winnerName = scoreA >= scoreB ? teamA.Name : teamB.Name;

           
            var mvp = box.Values
                .OrderByDescending(b => b.TeamId == winnerId)
                .ThenByDescending(b => b.PerformanceIndex)
                .First();

            return new MatchSimulationResult
            {
                TeamAId = teamA.Id,
                TeamBId = teamB.Id,
                TeamAName = teamA.Name,
                TeamBName = teamB.Name,
                ScoreA = scoreA,
                ScoreB = scoreB,
                WinnerTeamId = winnerId,
                WinnerTeamName = winnerName,
                MvpPlayerId = mvp.PlayerId,
                MvpPlayerName = mvp.PlayerName,
                BoxScores = box.Values.OrderByDescending(b => b.Points).ToList(),
                PlayByPlay = playByPlay
            };
        }

        private bool IsGameOver(int a, int b)
        {
            if (a >= HardCap || b >= HardCap) return true;
            if ((a >= TargetScore || b >= TargetScore) && Math.Abs(a - b) >= 2) return true;
            return false;
        }

        private static List<Player> StartingFive(Team team)
        {
            return (team.Players ?? new List<Player>())
                .OrderByDescending(p => p.OverallRating)
                .Take(5)
                .ToList();
        }

        
        private int RunPossession(
            List<Player> offense,
            List<Player> defense,
            Dictionary<int, PlayerBoxScore> box,
            Random rnd,
            List<string> pbp,
            bool isTeamA)
        {
           
            var shooter = WeightedPick(offense, rnd, p => p.PaintScoringPercentage + p.MidRangePercentage + p.SpeedRating);
            var defender = WeightedPick(defense, rnd, p => p.DefenseRating + p.SpeedRating);

            
            double stealChance = Clamp((defender.DefenseRating - shooter.PassingRating) * 0.4 + 12, 3, 35) / 100.0;
            if (rnd.NextDouble() < stealChance)
            {
                box[defender.Id].Steals++;
                return 0;
            }

            
            bool attemptThree = rnd.NextDouble() < 0.32;
            int shotValue = attemptThree ? 2 : 1;

            int basePct = attemptThree
                ? shooter.ThreePointPercentage
                : (rnd.NextDouble() < 0.5 ? shooter.MidRangePercentage : shooter.PaintScoringPercentage);

          
            double defenseImpact = (defender.DefenseRating - 60) * 0.25;
            double effectivePct = Clamp(basePct - defenseImpact, 5, 97);

            var shooterBox = box[shooter.Id];
            shooterBox.FieldGoalsAttempted++;
            if (attemptThree) shooterBox.ThreePointersAttempted++;

        
            double blockChance = Clamp((defender.DefenseRating - 70) * 0.3, 0, 12) / 100.0;
            if (rnd.NextDouble() < blockChance)
            {
                box[defender.Id].Blocks++;
                box[defender.Id].Rebounds += rnd.NextDouble() < 0.5 ? 1 : 0;
                return 0;
            }

            bool made = rnd.NextDouble() * 100 < effectivePct;
            if (made)
            {
                shooterBox.FieldGoalsMade++;
                shooterBox.Points += shotValue;
                if (attemptThree) shooterBox.ThreePointersMade++;

                
                if (offense.Count > 1 && rnd.NextDouble() < 0.55)
                {
                    var passer = WeightedPick(
                        offense.Where(p => p.Id != shooter.Id).ToList(),
                        rnd, p => p.PassingRating);
                    if (passer != null) box[passer.Id].Assists++;
                }

                pbp.Add($"{shooter.FullName} scores {shotValue} ({(attemptThree ? "deep" : "inside")}).");
                return shotValue;
            }
            else
            {
                
                var allOnCourt = offense.Concat(defense).ToList();
                var rebounder = WeightedPick(allOnCourt, rnd, p => p.ReboundingRating);
                if (rebounder != null) box[rebounder.Id].Rebounds++;
                pbp.Add($"{shooter.FullName} misses; {rebounder?.FullName} grabs the board.");

                
                if (rnd.NextDouble() < 0.10)
                {
                    int ftPoints = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        shooterBox.FreeThrowsAttempted++;
                        if (rnd.NextDouble() * 100 < shooter.FreeThrowPercentage)
                        {
                            shooterBox.FreeThrowsMade++;
                            shooterBox.Points++;
                            ftPoints++;
                        }
                    }
                    return ftPoints;
                }
                return 0;
            }
        }

        private static T WeightedPick<T>(List<T> items, Random rnd, Func<T, double> weight)
        {
            if (items == null || items.Count == 0) return default;
            double total = items.Sum(i => Math.Max(1, weight(i)));
            double roll = rnd.NextDouble() * total;
            double cumulative = 0;
            foreach (var item in items)
            {
                cumulative += Math.Max(1, weight(item));
                if (roll <= cumulative) return item;
            }
            return items[^1];
        }

        private static double Clamp(double v, double min, double max) => Math.Max(min, Math.Min(max, v));
    }
}
