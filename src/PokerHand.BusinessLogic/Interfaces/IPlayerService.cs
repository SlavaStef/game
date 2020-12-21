using System;
using System.Threading.Tasks;
using PokerHand.Common.Dto;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto> AddNewPlayer(string playerName);
        Task<PlayerProfileDto> Authenticate(Guid playerId);
    }
}