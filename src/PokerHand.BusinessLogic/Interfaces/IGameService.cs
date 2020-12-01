using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public (Table, bool, Player) AddPlayerToTable(string userName, int maxPlayers);
        public (Table, bool, bool) RemovePlayerFromTable(string userName);
    }
}