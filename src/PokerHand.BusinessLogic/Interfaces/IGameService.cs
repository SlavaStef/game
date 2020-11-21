using System;
using System.Security.Claims;
using System.Threading.Tasks;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public (TableDto, bool) AddPlayerToTable(string userId);
    }
}