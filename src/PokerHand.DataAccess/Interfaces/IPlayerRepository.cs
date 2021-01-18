using System;
using System.Threading.Tasks;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IPlayerRepository
    {
        Task AddToTotalMoney(Guid playerId, int amount);
        Task SubtractFromTotalMoney(Guid playerId, int amount);
    }
}