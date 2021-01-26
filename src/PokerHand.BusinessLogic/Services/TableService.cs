using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Services
{
    public class TableService : ITableService
    {
        private readonly List<Table> _allTables;
        private readonly UserManager<Player> _userManager;
        private readonly IPlayerService _playerService;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;

        public TableService(
            TablesCollection tablesCollection,
            UserManager<Player> userManager,
            ILogger<TableService> logger, 
            IMapper mapper, 
            IPlayerService playerService)
        {
            _allTables = tablesCollection.Tables;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _playerService = playerService;
        }

        public TableInfoDto GetTableInfo(string tableName)
        {
            return new TableInfoDto
            {
                Title = tableName,
                TableType = TableOptions.Tables[tableName]["TableType"],
                Experience = TableOptions.Tables[tableName]["Experience"],
                SmallBlind = TableOptions.Tables[tableName]["SmallBlind"],
                BigBlind = TableOptions.Tables[tableName]["BigBlind"],
                MinBuyIn = TableOptions.Tables[tableName]["MinBuyIn"],
                MaxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"],
                MaxPlayers = TableOptions.Tables[tableName]["MaxPlayers"]
            };
        }

        public List<TableInfoDto> GetAllTablesInfo()
        {
            var allTablesInfo = new List<TableInfoDto>();

            foreach (var table in TableOptions.Tables)
            {
                var tableInfoDto = new TableInfoDto
                {
                    Title = table.Key,
                    TableType = table.Value["TableType"],
                    Experience = table.Value["Experience"],
                    SmallBlind = table.Value["SmallBlind"],
                    BigBlind = table.Value["BigBlind"],
                    MinBuyIn = table.Value["MinBuyIn"],
                    MaxBuyIn = table.Value["MaxBuyIn"],
                    MaxPlayers = table.Value["MaxPlayers"]
                };
                
                allTablesInfo.Add(tableInfoDto);
            }

            return allTablesInfo;
        }

        public async Task<(TableDto, bool, PlayerDto)> AddPlayerToTable(TableTitle tableTitle, Guid playerId, 
            string playerConnectionId, int buyInAmount, bool isAutoTop)
        {
            var table = GetFreeTable(tableTitle);
            var isNewTable = false;

            if (table == null) // Create new table if there are no free required tables
            {
                table = CreateNewTable(tableTitle);
                isNewTable = true;
            }

            _logger.LogInformation($"table: {JsonSerializer.Serialize(table)}");
            
            var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);
            player.ConnectionId = playerConnectionId;
            player.IndexNumber = isNewTable ? 0 : GetFreeSeatIndex(table);
            player.IsAutoTop = isAutoTop;
            player.CurrentBuyIn = buyInAmount;

            if (player.TotalMoney >= buyInAmount)
            {
                await _playerService.GetStackMoney(player.Id, buyInAmount);
                player.StackMoney = buyInAmount;
            }
                
            if (player.StackMoney == 0)
                return (null, false, _mapper.Map<PlayerDto>(player));
            
            _logger.LogInformation($"table: {JsonSerializer.Serialize(table)}");
            table.Players.Add(player);
            _logger.LogInformation($"table: {JsonSerializer.Serialize(table)}");
            table.Players = table.Players.OrderBy(p => p.IndexNumber).ToList();
            _logger.LogInformation($"table: {JsonSerializer.Serialize(table)}");
            
            var tableDto = _mapper.Map<TableDto>(table);
            _logger.LogInformation($"tableDto: {JsonSerializer.Serialize(tableDto)}");
            var playerDto = _mapper.Map<PlayerDto>(player);
            
            return (tableDto, isNewTable, playerDto);
        }

        public async Task<TableDto> RemovePlayerFromTable(Guid tableId, Guid playerId)
        {
            _logger.LogInformation($"Method RemovePlayerFromTable. Start");
            
            var table = _allTables.First(t => t.Id == tableId);
            var playerFromTable = table.Players.First(p => p.Id == playerId);
            
            table.Pot += playerFromTable.CurrentBet;

            await _playerService.ReturnToTotalMoney(playerId, playerFromTable.StackMoney);
            
            table.Players.Remove(playerFromTable);
            
            if(table.ActivePlayers.Contains(playerFromTable))
                table.ActivePlayers.Remove(playerFromTable);
            
            _logger.LogInformation($"Player was removed from table");
            
            //TODO: If one of two players leaves round -> stop round & the second player is the winner
            if (table.Players.Count == 1)
            {
                table.WaitForPlayerBet.Set();
            }

            // If there is no player at the table -> delete this table
            if (table.Players.Count == 0)
            {
                _allTables.Remove(table);
                table.Dispose();
                _logger.LogInformation("Table deleted from all tables");
                return null;
            }
            
            var resultTable = _mapper.Map<TableDto>(table);
            _logger.LogInformation($"Method RemovePlayerFromTable. resultTableDto: {JsonSerializer.Serialize(resultTable)}");
            
            _logger.LogInformation($"Method RemovePlayerFromTable ends");
            return resultTable;
        }

        #region privateHelpers

        // Get a required table with free seats
        private Table GetFreeTable(TableTitle tableType) => 
            _allTables?
                .Where(t => t.Title == tableType)
                .FirstOrDefault(table => table.Players.Count < table.MaxPlayers);

        private Table CreateNewTable(TableTitle tableTitle)
        {
            var newTable = new Table(tableTitle);
            _allTables.Add(newTable);
            
            return newTable;
        }

        private static int GetFreeSeatIndex(Table table)
        {
            // table seats counter starts from 0
            var seatIndex = -1;

            for (var i = 0; i < table.MaxPlayers; i++)
            {
                if (table.Players.Any(player => player.IndexNumber == i)) 
                    continue;
                
                seatIndex = i;
                break;
            }

            return seatIndex;
        }
        
        #endregion
    }
}