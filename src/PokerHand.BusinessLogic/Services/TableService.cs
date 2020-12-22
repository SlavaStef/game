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

namespace PokerHand.BusinessLogic.Services
{
    public class TableService : ITableService
    {
        private readonly List<Table> _allTables;
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;

        public TableService(
            TablesCollection tablesCollection,
            UserManager<Player> userManager,
            ILogger<TableService> logger, 
            IMapper mapper)
        {
            _allTables = tablesCollection.Tables;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
        }

        public TableInfoDto GetTableInfo(string tableName)
        {
            return new TableInfoDto
            {
                Experience = TableOptions.Tables[tableName]["Experience"],
                SmallBlind = TableOptions.Tables[tableName]["SmallBlind"],
                BigBlind = TableOptions.Tables[tableName]["BigBlind"],
                MinBuyIn = TableOptions.Tables[tableName]["MinBuyIn"],
                MaxBuyIn = TableOptions.Tables[tableName]["MaxBuyIn"],
                MaxPlayers = TableOptions.Tables[tableName]["MaxPlayers"]
            };
        }

        public async Task<(TableDto, bool, PlayerDto)> AddPlayerToTable(TableTitle tableTitle, Guid playerId, int buyInAmount)
        {
            var table = GetFreeTable(tableTitle);
            var isNewTable = false;

            if (table == null) // Create new table if there are no free required tables
            {
                table = CreateNewTable(tableTitle);
                isNewTable = true;
            }

            var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);
            player.IndexNumber = isNewTable ? 0 : GetFreeSeatIndex(table);
            player.StackMoney = buyInAmount;
            
            table.Players.Add(player);
            table.Players = table.Players.OrderBy(p => p.IndexNumber).ToList();

            var tableDto = _mapper.Map<TableDto>(table);
            var playerDto = _mapper.Map<PlayerDto>(player);
            
            return (tableDto, isNewTable, playerDto);
        }

        public (Table, bool) RemovePlayerFromTable(string userName)
        {
            _logger.LogInformation($"Method RemovePlayerFromTable starts");
            
            _logger.LogInformation($"Current tables in application:");
            foreach (var _table in _allTables)
            {
                _logger.LogInformation($"Table {_table.Id}. Players: {_table.Players.Count}");
                _logger.LogInformation("Players:");
                foreach (var player in _table.Players)
                {
                    _logger.LogInformation($"Player {player.Id}, name: {player.UserName}");
                }
            }
            
            var table = _allTables
                .First(t => t.Players.FirstOrDefault(player => player.UserName == userName) != null);
            //_logger.LogInformation($"Table to remove player from: {table.Id}");
            var playerToRemove = table.Players.First(player => player.UserName == userName);
            //_logger.LogInformation($"Player to remove: {playerToRemove.Id}");
            
            table.Players.Remove(playerToRemove);
            table.ActivePlayers.Remove(playerToRemove);
            _logger.LogInformation($"Player was removed from table");
            
            var isPlayerRemoved = !table.Players.Contains(playerToRemove);
            
            //TODO: If one of two players leaves round -> stop round & the second player is the winner
            if (table.Players.Count == 1)
            {
                
            }

            // if there is no player at a table -> delete this table
            if (table.Players.Count == 0)
            {
                _allTables.Remove(table);
                table.Dispose();
                _logger.LogInformation($"{JsonSerializer.Serialize(_allTables)}");
                _logger.LogInformation("Table deleted from all tables");
                return (null, isPlayerRemoved);
            }
            
            _logger.LogInformation($"Method RemovePlayerFromTable ends");
            return (table, isPlayerRemoved);
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