using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

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
            table.IsInGame = true;
            
            SetDealerAndBlinds(ref table);
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

        private static void SetDealerAndBlinds(ref Table table)
        {
            table.Players[0].Button = ButtonTypeNumber.Dealer;
            table.Players[1].Button = ButtonTypeNumber.BigBlind;
            table.Players[2].Button = ButtonTypeNumber.SmallBlind;
            
            DealPocketCards(ref table);
        }

        private static void DealPocketCards(ref Table table)
        {
            foreach (var player in table.Players)
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);
            
            StartPreFlopWagering(ref table);
        }

        // Pre-flop
        private static void StartPreFlopWagering(ref Table table)
        {
            
        }
        
        // Flop, Turn, River
        public void StartWagering()
        {
            
        }

        private static void PutCardsOnTable(ref Table table, int numberOfCards)
        {
            var cardToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            table.CommunityCards.AddRange(cardToAdd);
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