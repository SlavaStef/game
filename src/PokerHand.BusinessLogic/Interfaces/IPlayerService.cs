using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PokerHand.Common.Dto;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto> AddNewPlayer(string playerName, ILogger logger);
        Task<PlayerProfileDto> Authenticate(Guid playerId);
    }
}