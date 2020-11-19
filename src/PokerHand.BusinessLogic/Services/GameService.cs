using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        
        public void StartRound(Guid tableId)
        {
            var table = _allTables.First(t => t.Id == tableId);
            
            while (table.Players.Count > 0)
            {
                MakeBets(table.Players);
            }
        }

        public (TableDto, bool) AddPlayerToTable(string user)
        {
            var table = GetFreeTable();
            bool isNewTable = false;

            if (table == null)
            {
                table = CreateNewTable();
                isNewTable = true;
                return (_mapper.Map<TableDto>(table), isNewTable);
            }

            return (_mapper.Map<TableDto>(table), isNewTable);
        }

        private void MakeBets(List<Player> players)
        {
            foreach (var player in players)
            {
                
            }
        }

        public void SetDealerAndBlinds()
        {
            
        }

        public void DealPocketCards()
        {
            
        }

        // Pre-flop
        public void StartPreFlopWagering()
        {
            
        }
        
        // Flop, Turn, River
        public void StartWagering()
        {
            
        }

        public void GetFlopCards()
        {
            
        }

        // Turn, River
        public void GetCard()
        {
            
        }

        public void ProcessShowdown()
        {
            
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