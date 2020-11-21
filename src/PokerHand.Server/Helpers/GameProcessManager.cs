using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Server.Hubs;

namespace PokerHand.Server.Helpers
{
    public interface IGameProcessManager
    {
        public Task StartRound(Guid tableId);
    }
    
    public class GameProcessManager : IGameProcessManager
    {
        private List<Table> _allTables;
        private readonly IHubContext<GameHub> _hub;
        private readonly IGameService _gameService;
        private readonly IMapper _mapper;

        public GameProcessManager(
            TablesCollection tablesCollection,
            IGameService gameService,
            IHubContext<GameHub> hubContext,
            IMapper mapper)
        {
            _allTables = tablesCollection.Tables;
            _hub = hubContext;
            _gameService = gameService;
            _mapper = mapper;
        }
        
        public async Task StartRound(Guid tableId)
        {
            Table table = _allTables.First(t => t.Id == tableId);
            table.IsInGame = true;
            
            await SetDealerAndBlinds(table);
            await DealPocketCards(table);
        }
        
        private async Task SetDealerAndBlinds(Table table)
        {
            switch (table.Players.Count)
            {
                case 1:
                    break;
                case 2:
                    table.Players[0].Button = ButtonTypeNumber.BigBlind;
                    table.Players[1].Button = ButtonTypeNumber.SmallBlind;
                    break;
                default:
                    table.Players[0].Button = ButtonTypeNumber.Dealer;
                    table.Players[1].Button = ButtonTypeNumber.BigBlind;
                    table.Players[2].Button = ButtonTypeNumber.SmallBlind;
                    break; 
            }

            await _hub.Clients.Group(table.Id.ToString()).SendAsync("SetDealerAndBlinds", _mapper.Map<List<PlayerDto>>(table.Players));
        }
        
        private async Task DealPocketCards(Table table)
        {
            foreach (var player in table.Players)
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);

            await _hub.Clients.Group(table.Id.ToString()).SendAsync("DealPocketCards", _mapper.Map<List<PlayerDto>>(table.Players));
        }

        private async Task StartPreFlopWagering(Table table)
        {
            bool areBetsEqual = false;

            int firstPlayerId = DefinePlayerToStartWagering(table.Players);
            
            while (!areBetsEqual)
            {
                for (int i = firstPlayerId; i < firstPlayerId + table.Players.Count; i++)
                {
                    table.CurrentPlayer = table.Players.First(player => player.IndexNumber == i);
                    await _hub.Clients.Group(table.Id.ToString())
                        .SendAsync("ReceiveCurrentPlayerId", _mapper.Map<Guid>(table.CurrentPlayer.Id));
                    // TODO: add countdown
                    Waiter.waitForPlayerBet.WaitOne();
                    // Process player's choice: Fold, Check, Raise
                }

                if (CheckIfBetsAreEqual(table.Players))
                    break;
            }

            
        }

        #region PrivateHelpers

        private static bool CheckIfBetsAreEqual(List<Player> players) =>
            players.All(player => player.CurrentBet == players[0].CurrentBet);

        private int DefinePlayerToStartWagering(List<Player> players)
        {
            var bigBlindPosition = 
                players.First(player => player.Button == ButtonTypeNumber.BigBlind).IndexNumber;

            if (bigBlindPosition == players.Count)
                return 1;

            return bigBlindPosition + 1;
        }
        
        #endregion
    }
}