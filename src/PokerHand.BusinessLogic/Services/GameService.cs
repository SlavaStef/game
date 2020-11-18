using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
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
        
        public TableDto AddPlayerToTable(string user)
        {
            var table = GetFreeTable() ?? CreateNewTable();

            return _mapper.Map<TableDto>(table);
        }

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
            Table newTable = new Table();
            _allTables.Add(newTable);

            return newTable;
        }
        
        
    }
}