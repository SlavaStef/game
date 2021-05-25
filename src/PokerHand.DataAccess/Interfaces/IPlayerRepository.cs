using System;
using System.Threading.Tasks;
using PokerHand.Common.Entities;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IPlayerRepository : IRepository<Player>
    {
        Task<Player> GetPlayerAsync(Guid playerId);
        Task<bool> PlayerExistsAsync(Guid playerId);
        
        Task AddTotalMoneyAsync(Guid playerId, int amount);
        Task SubtractTotalMoneyAsync(Guid playerId, int amount);
        Task AddCoinsAsync(Guid playerId, int amount);
        Task SubtractCoinsAsync(Guid playerId, int amount);

        Task<int> GetMoneyBoxAmountAsync(Guid playerId);
        Task<int> IncreaseMoneyBoxAmountAsync(Guid playerId, int amount);
        Task<int> OpenMoneyBoxAsync(Guid playerId);
        
        Task IncreaseNumberOfPlayedGamesAsync(Guid playerId, bool isWin);
        Task IncreaseNumberOfSitNGoWinsAsync(Guid playerId);
        Task ChangeBestHandTypeAsync(Guid playerId, int newHandType);
        Task ChangeBiggestWinAsync(Guid playerId, int newBiggestWin);
        Task AddExperienceAsync(Guid playerId, int numberOfExperience);
    }
}