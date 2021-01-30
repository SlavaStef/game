using System;
using System.Threading.Tasks;
using PokerHand.Common.Dto;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto> AddNewPlayer(string playerName);
        Task<PlayerProfileDto> Authenticate(Guid playerId);

        Task<bool> GetStackMoney(Guid playerId, int amount);
        Task<bool> AddStackMoneyFromTotalMoney(Guid tableId, Guid playerId, int requiredAmount);
        Task ReturnToTotalMoney(Guid playerId, int amountToAdd);
        Task<PlayerProfileDto> GetPlayerProfile(Guid playerId);
        void SetPlayerReady(Guid tableId, Guid playerId);
    }
}