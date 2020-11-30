using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Entities;

namespace PokerHand.BusinessLogic.Services
{
    public class GameService : IGameService
    {
        private List<Table> _allTables;
        private readonly IMapper _mapper;

        public GameService(
            TablesCollection tablesCollection,
            IMapper mapper)
        {
            _allTables = tablesCollection.Tables;
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

            table.Players = table.Players.OrderBy(player => player.IndexNumber).ToList();

            return (table, isNewTable, player);
        }

        public (Table, bool, bool) RemovePlayerFromTable(string userName)
        {
            var table = _allTables.First(table => table.Players.FirstOrDefault(player => player.UserName == userName) != null);
            var playerToRemove = table.Players.First(player => player.UserName == userName);

            table.Players.Remove(playerToRemove);
            table.ActivePlayers.Remove(playerToRemove);

            var isPlayerRemoved = !table.Players.Contains(playerToRemove);

            var isTableRemoved = table.Players.Count == 0;

            return (table, isPlayerRemoved, isTableRemoved);
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