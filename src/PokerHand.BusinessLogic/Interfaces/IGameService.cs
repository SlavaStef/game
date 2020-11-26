using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public (Table, bool) AddPlayerToTable(string userId);
        public (Table, bool, bool) RemovePlayerFromTable(string userName);
    }
}