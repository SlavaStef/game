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
using PokerHand.Common.Helpers;
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

        public async Task<(TableDto tableDto, bool isNewTable, PlayerDto playerDto)> AddPlayerToTable(
            TableTitle tableTitle, Guid playerId, string playerConnectionId, int buyInAmount, bool isAutoTop)
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

            if (table.Type == TableType.SitAndGo)
            {
                player.IsAutoTop = false;
                player.CurrentBuyIn = buyInAmount;

                await _playerService.GetStackMoney(player.Id, buyInAmount);
                player.StackMoney = 3000;
            }
            else
            {
                player.IsAutoTop = isAutoTop;
                player.CurrentBuyIn = buyInAmount;
                
                if (player.TotalMoney >= buyInAmount)
                {
                    await _playerService.GetStackMoney(player.Id, buyInAmount);
                    player.StackMoney = buyInAmount;
                }
                
                if (player.StackMoney == 0)
                    return (null, false, _mapper.Map<PlayerDto>(player));
            }

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
            var table = _allTables.First(t => t.Id == tableId);
            var player = table.Players.First(p => p.Id == playerId);
            
            // Deal with player's state on table
            table.Pot += player.CurrentBet;
            
            if (player.StackMoney > 0) 
                await _playerService.ReturnToTotalMoney(playerId, player.StackMoney);
            
            if (table.ActivePlayers.Contains(player))
                table.ActivePlayers.Remove(player);
            
            table.Players.Remove(player);
            
            _logger.LogInformation($"Player was removed from table");
            
            // Deal with state of table if player's removal effects it
            // Two not ordinary situations:
            // 1. One player left on table -> end game and wait for a new player to join
            // 2. No players left on table -> delete this table
            
            //TODO: Implement this logics
            if (table.Players.Count == 1)
            {
                table.WaitForPlayerBet.Set();
            }

            if (table.Players.Count == 0)
            {
                _allTables.Remove(table);
                table.Dispose();
                _logger.LogInformation("Table was removed from all tables list");
                return null;
            }
            
            return _mapper.Map<TableDto>(table);
        }

        public async Task<TableDto> RemovePlayerFromSitAndGoTable(Guid tableId, Guid playerId)
        {
            var table = _allTables.First(t => t.Id == tableId);
            var playerFromTable = table.Players.First(p => p.Id == playerId);

            table.ActivePlayers.Remove(playerFromTable);
            table.Players.Remove(playerFromTable);

            return _mapper.Map<TableDto>(table);
        }

        public void RemoveTableById(Guid tableId)
        {
            var tableToRemove = _allTables.FirstOrDefault(t => t.Id == tableId);

            if (tableToRemove != null)
                _allTables.Remove(tableToRemove);
        }

        #region privateHelpers

        // Get a required table with free seats
        private Table GetFreeTable(TableTitle tableTitle)
        {
            if (tableTitle == TableTitle.RivieraHotel ||
                tableTitle == TableTitle.CityDreamsResort ||
                tableTitle == TableTitle.HeritageBank)
            {
                return _allTables?
                    .Where(t => t.Title == tableTitle)
                    .FirstOrDefault(t => t.CurrentStage == RoundStageType.None);
            }
            
            return _allTables?
                .Where(t => t.Title == tableTitle)
                .FirstOrDefault(t => t.Players.Count < t.MaxPlayers);
        }

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