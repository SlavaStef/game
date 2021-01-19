using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto> AddNewPlayer(string playerName, ILogger logger);
        Task<PlayerProfileDto> Authenticate(Guid playerId);

        Task<int> GetStackMoney(Guid playerId, int amount);
        Task ReturnToTotalMoney(Guid playerId, int amountToAdd);
    }
}