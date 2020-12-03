using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Services
{
    public class GameService : IGameService
    {
        private List<Table> _allTables;
        private readonly ILogger<GameService> _logger;
        private readonly IMapper _mapper;

        public GameService(
            TablesCollection tablesCollection,
            ILogger<GameService> logger,
            IMapper mapper)
        {
            _allTables = tablesCollection.Tables;
            _logger = logger;
            _mapper = mapper;
        }
 
        public (Table, bool, Player) AddPlayerToTable(string userName, int maxPlayers)
        {
            var table = GetFreeTable();
            var isNewTable = false;

            if (table == null)
            {
                table = CreateNewTable(maxPlayers);
                isNewTable = true;
            }

            // Define player's position at the table (starts with 0)
            var player = new Player
            {
                UserName = userName,
                IndexNumber = isNewTable ? 0 : GetFreeSeatIndex(table)
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
            
            var isPlayerRemoved = !table.Players.Contains(playerToRemove);
            
            if (table.Players.Count == 0)
            {
                _logger.LogInformation($"No players in table {table.Id}");
                _allTables.Remove(table);
                _logger.LogInformation("Table deleted from all tables");
                return (null, isPlayerRemoved);
            }
            
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

            _logger.LogInformation($"Method RemovePlayerFromTable ends");
            return (table, isPlayerRemoved);
        }

        #region privateHelpers

        private Table GetFreeTable() => 
            _allTables?.FirstOrDefault(table => table.Players.Count < table.MaxPlayers);

        private Table CreateNewTable(int maxPlayers)
        {
            var newTable = new Table(maxPlayers);
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