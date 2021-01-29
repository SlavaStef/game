﻿using System;
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
        Task<(TableDto tableDto, bool isNewTable, PlayerDto playerDto)> AddPlayerToTable(TableTitle tableTitle, Guid playerId, string playerConnectionId, int buyIn, bool isAutoTop);
        Task<TableDto> RemovePlayerFromTable(Guid tableId, Guid playerId);
        Task<TableDto> RemovePlayerFromSitAndGoTable(Guid tableId, Guid playerId);
        void RemoveTable(Guid tableId);
    }
}