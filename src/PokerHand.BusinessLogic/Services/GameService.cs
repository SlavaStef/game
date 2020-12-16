using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Services
{
    public class GameService : IGameService
    {
        private readonly List<Table> _allTables;
        private readonly ILogger<GameService> _logger;

        public GameService(
            TablesCollection tablesCollection,
            ILogger<GameService> logger)
        {
            _allTables = tablesCollection.Tables;
            _logger = logger;
        }

        public TableInfoDto GetTableInfo(string tableName)
        {
            throw new System.NotImplementedException();
        }

        public (Table, bool, Player) AddPlayerToTable(string userName, TableTitle tableTitle, int buyIn)
        {
            var table = GetFreeTable(tableTitle);
            var isNewTable = false;

            if (table == null) // Create new table if there are no free required tables
            {
                table = CreateNewTable(tableTitle);
                isNewTable = true;
            }

            // Define player's position at the table (starts with 0)
            var player = new Player
            {
                UserName = userName,
                IndexNumber = isNewTable ? 0 : GetFreeSeatIndex(table),
                StackMoney = buyIn
            };
            
            table.Players.Add(player);

            table.Players = table.Players.OrderBy(p => p.IndexNumber).ToList();

            return (table, isNewTable, player);
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