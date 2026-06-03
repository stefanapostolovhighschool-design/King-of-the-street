using KingOfTheStreet.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<MVPVote> MVPVotes { get; set; }
        public DbSet<PlayerMatchStat> PlayerMatchStats { get; set; }
        public DbSet<TournamentRegistration> TournamentRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Player>()
                .Ignore(p => p.OverallRating);

           
            builder.Entity<Team>()
                .HasOne(t => t.Captain)
                .WithMany(u => u.CaptainedTeams)
                .HasForeignKey(t => t.CaptainId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            
            builder.Entity<Match>()
                .HasOne(m => m.TeamA)
                .WithMany()
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Match>()
                .HasOne(m => m.TeamB)
                .WithMany()
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Match>()
                .HasOne(m => m.Winner)
                .WithMany()
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Match>()
                .HasOne(m => m.Court)
                .WithMany(c => c.Matches)
                .HasForeignKey(m => m.CourtId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Match>()
                .HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Match>()
                .HasOne(m => m.MvpPlayer)
                .WithMany()
                .HasForeignKey(m => m.MvpPlayerId)
                .OnDelete(DeleteBehavior.Restrict);

           
            builder.Entity<PlayerMatchStat>()
                .HasOne(s => s.Match)
                .WithMany(m => m.PlayerStats)
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PlayerMatchStat>()
                .HasOne(s => s.Player)
                .WithMany(p => p.MatchStats)
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<MVPVote>()
                .HasIndex(v => new { v.UserId, v.TournamentId })
                .IsUnique();

            builder.Entity<MVPVote>()
                .HasOne(v => v.User)
                .WithMany(u => u.MVPVotes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MVPVote>()
                .HasOne(v => v.Player)
                .WithMany()
                .HasForeignKey(v => v.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MVPVote>()
                .HasOne(v => v.Tournament)
                .WithMany(t => t.MVPVotes)
                .HasForeignKey(v => v.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<TournamentRegistration>()
                .HasOne(r => r.Tournament)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TournamentRegistration>()
                .HasOne(r => r.Team)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
