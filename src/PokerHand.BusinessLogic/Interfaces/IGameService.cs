using System.Security.Claims;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public TableDto AddPlayerToTable(string userId);
    }
}