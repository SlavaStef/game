using PokerHand.Common.Dto;
using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public TableInfoDto GetTableInfo(string tableName);
        public (Table, bool, Player) AddPlayerToTable(string userName, int maxPlayers);
        public (Table, bool) RemovePlayerFromTable(string userName);
    }
}