using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IGameService
    {
        public TableInfoDto GetTableInfo(string tableName);
        public (Table, bool, Player) AddPlayerToTable(string userName, TableTitle tableTitle, int buyIn);
        public (Table, bool) RemovePlayerFromTable(string userName);
    }
}