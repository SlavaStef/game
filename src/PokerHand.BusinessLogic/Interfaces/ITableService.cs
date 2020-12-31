﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface ITableService
    {
        public TableInfoDto GetTableInfo(string tableName);
        public List<TableInfoDto> GetAllTablesInfo();
        public Task<(TableDto, bool, PlayerDto)> AddPlayerToTable(TableTitle tableTitle, Guid playerId, int buyIn);
        public Task<TableDto> RemovePlayerFromTable(Guid tableId, Guid playerId);
    }
}