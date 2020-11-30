using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public (Table, bool, Player) AddPlayerToTable(string userId, int maxPlayers);
        public (Table, bool, bool) RemovePlayerFromTable(string userName);
    }
}