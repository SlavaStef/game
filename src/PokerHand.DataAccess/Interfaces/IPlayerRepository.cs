using System;
using System.Threading.Tasks;
using PokerHand.Common.Entities;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player> GetPlayerAsync(Guid playerId);
        Task<bool> PlayerExistsAsync(Guid id);
        
        Task AddTotalMoneyAsync(Guid playerId, int amount);
        Task SubtractTotalMoneyAsync(Guid playerId, int amount);
        Task AddCoinsAsync(Guid playerId, int amount);
        Task SubtractCoinsAsync(Guid playerId, int amount);
        Task IncreaseNumberOfPlayedGamedAsync(Guid playerId, bool isWin);
        Task IncreaseNumberOfSitNGoWinsAsync(Guid playerId);
        Task ChangeBestHandTypeAsync(Guid playerId, int newHandType);
        Task ChangeBiggestWinAsync(Guid playerId, int newBiggestWin);
        Task AddExperienceAsync(Guid playerId, int numberOfExperience);
    }
}