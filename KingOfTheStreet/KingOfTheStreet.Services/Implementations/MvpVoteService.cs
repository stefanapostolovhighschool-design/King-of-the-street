using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KingOfTheStreet.Services.Implementations
{
    public class MvpVoteService : IMvpVoteService
    {
        private readonly ApplicationDbContext _context;
        public MvpVoteService(ApplicationDbContext context) => _context = context;

        public async Task<bool> VoteAsync(string userId, int tournamentId, int playerId)
        {
            bool alreadyVoted = await _context.MVPVotes
                .AnyAsync(v => v.UserId == userId && v.TournamentId == tournamentId);
            if (alreadyVoted) return false;

            _context.MVPVotes.Add(new MVPVote
            {
                UserId = userId,
                TournamentId = tournamentId,
                PlayerId = playerId
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetVoteCountAsync(int playerId, int tournamentId) =>
            await _context.MVPVotes.CountAsync(v => v.PlayerId == playerId && v.TournamentId == tournamentId);
    }
}
