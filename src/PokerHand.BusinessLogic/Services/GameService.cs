using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
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
 
        public (TableDto, bool) AddPlayerToTable(string userName)
        {
            var table = GetFreeTable();
            bool isNewTable = false;

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

            return (_mapper.Map<TableDto>(table), isNewTable);
        }

        #region privateHelpers

        private Table GetFreeTable()
        {
            Table freeTable = null;
            
            foreach (var table in _allTables)
            {
                if (table.Players.Count < table.MaxPlayers)
                {
                    freeTable = table;
                    break;
                }
            }

            return freeTable;
        }

        private Table CreateNewTable()
        {
            var maxPlayers = 2;
            Table newTable = new Table(maxPlayers);
            _allTables.Add(newTable);
            
            return newTable;
        }

#endregion
    }
}