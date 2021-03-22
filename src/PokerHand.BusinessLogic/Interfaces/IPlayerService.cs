using System;
using System.Threading.Tasks;
using PokerHand.Common.Dto;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto> CreatePlayer(string playerName);
        Task<PlayerProfileDto> Authenticate(Guid playerId);

        Task<bool> GetFromTotalMoney(Guid playerId, int amount);
        Task<bool> AddStackMoneyFromTotalMoney(Guid tableId, Guid playerId, int requiredAmount);
        Task AddTotalMoney(Guid playerId, int amountToAdd);
        Task<PlayerProfileDto> GetPlayerProfile(Guid playerId);
        void SetPlayerReady(Guid tableId, Guid playerId);
        void ChangeAutoTop(Guid tableId, Guid playerId, bool isAutoTop);

        Task IncreaseNumberOfPlayedGamesAsync(Guid playerId, bool isWin);
        Task IncreaseNumberOfSitNGoWinsAsync(Guid playerId);
        Task ChangeBestHandTypeAsync(Guid playerId, int newHandType);
        Task ChangeBiggestWinAsync(Guid playerId, int newBiggestWin);
        Task AddExperienceAsync(Guid playerId, int experience);
    }
}