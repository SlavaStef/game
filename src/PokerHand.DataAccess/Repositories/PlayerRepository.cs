using System;
using System.Linq;
using System.Threading.Tasks;
using PokerHand.Common.Entities;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.DataAccess.Repositories
{
    public class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        public PlayerRepository(ApplicationContext context) : base(context) { }

        public async Task AddToTotalMoney(Guid playerId, int amount)
        {
            var player = _context.Players.First(p => p.Id == playerId);

            player.TotalMoney += amount;

            await _context.SaveChangesAsync();
        }
        
        public async Task SubtractFromTotalMoney(Guid playerId, int amount)
        {
            var player = _context.Players.First(p => p.Id == playerId);

            player.TotalMoney -= amount;

            await _context.SaveChangesAsync();
        }
    }
}