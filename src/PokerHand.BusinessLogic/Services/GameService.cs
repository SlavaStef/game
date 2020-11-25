using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
 
        public (Table, bool) AddPlayerToTable(string userName)
        {
            var table = GetFreeTable();
            var isNewTable = false;

            if (table == null)
            {
                table = CreateNewTable();
                isNewTable = true;
            }

            // Define player's position at the table
            var player = new Player
            {
                UserName = userName,
                IndexNumber = table.Players.Count + 1
            };
            
            table.Players.Add(player);

            return (table, isNewTable);
        }

        #region privateHelpers

        private Table GetFreeTable() => 
            _allTables?.FirstOrDefault(table => table.Players.Count < table.MaxPlayers);

        private Table CreateNewTable()
        {
            // TODO: change usage of maxPlayers
            const int maxPlayers = 2;
            var newTable = new Table(maxPlayers);
            _allTables.Add(newTable);
            
            return newTable;
        }
        
        #endregion
    }
}