using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface ITableService
    {
        TableInfoDto GetTableInfo(string tableName);
        List<TableInfoDto> GetAllTablesInfo();
        Task<(TableDto tableDto, PlayerDto playerDto, bool isNewTable)> AddPlayerToTable(TableConnectionOptions options);
        Task<TableDto> RemovePlayerFromTable(Guid tableId, Guid playerId);
        void RemoveTableById(Guid tableId);
    }
}