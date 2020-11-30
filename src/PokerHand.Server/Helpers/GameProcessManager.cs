using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GameHub> _logger;

        public GameProcessManager(
            TablesCollection tablesCollection,
            IGameService gameService,
            IHubContext<GameHub> hubContext,
            IMapper mapper,
            ILogger<GameHub> logger)
        {
            _allTables = tablesCollection.Tables;
            _hub = hubContext;
            _gameService = gameService;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task StartRound(Guid tableId)
        {
            var table = _allTables.First(table => table.Id == tableId);

            if (table.Players.Count < 2)
            {
                _logger.LogInformation($"table {table.Id}: Round started. Waiting for two players");
                await _hub.Clients.Group(table.Id.ToString()).SendAsync("WaitForPlayers");
            }

            while (table.Players.Count < 2)
                System.Threading.Thread.Sleep(1000);
            
            _logger.LogInformation($"table {table.Id}: Game started");

            table.ActivePlayers = table.Players.ToList();
            table.IsInGame = true;
            
            await SetDealerAndBlinds(table);
            await DealPocketCards(table);
            await StartPreFlopWagering(table);
            await DealCommunityCards(table, 3);
            await StartWagering(table); // Deal 2-nd wagering
            await DealCommunityCards(table, 1); // Deal Turn card
            await StartWagering(table); // Deal 3-d wagering
            await DealCommunityCards(table, 1); // Deal River card
            await StartWagering(table); // Deal 4-th wagering
            await Showdown(table);

            table.IsInGame = false;
            table.ActivePlayers = null;
        }

        private async Task SetDealerAndBlinds(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method SetDealerAndBlinds starts");
            
            switch (table.ActivePlayers.Count)
            {
                case 2:
                    if (table.DealerIndex < 0 && table.SmallBlindIndex < 0 && table.BigBlindIndex < 0 ||
                        table.SmallBlindIndex == 1)
                    {
                        table.SmallBlindIndex = 0;
                        table.ActivePlayers.First(player => player.IndexNumber == 0).Button =
                            ButtonTypeNumber.SmallBlind;
                        
                        table.BigBlindIndex = 1;
                        table.ActivePlayers.First(player => player.IndexNumber == 1).Button =
                            ButtonTypeNumber.BigBlind;
                    }
                    else if (table.SmallBlindIndex == 0)
                    {
                        table.BigBlindIndex = 0;
                        table.ActivePlayers.First(player => player.IndexNumber == 0).Button =
                            ButtonTypeNumber.BigBlind;
                        
                        table.SmallBlindIndex = 1;
                        table.ActivePlayers.First(player => player.IndexNumber == 1).Button =
                            ButtonTypeNumber.SmallBlind;
                    }
                    break;
                
                default:
                    if (table.DealerIndex < 0 && table.SmallBlindIndex < 0 && table.BigBlindIndex < 0)
                    {
                        table.DealerIndex = 0;
                        table.ActivePlayers[0].Button = ButtonTypeNumber.Dealer;
                        
                        table.SmallBlindIndex = 1;
                        table.ActivePlayers[1].Button = ButtonTypeNumber.SmallBlind;
                        
                        table.BigBlindIndex = 2;
                        table.ActivePlayers[2].Button = ButtonTypeNumber.BigBlind;
                    }
                    else
                    {
                       var dealer = table.ActivePlayers.First(player => player.Button == ButtonTypeNumber.Dealer);
                       var nextDealer = dealer.IndexNumber == table.MaxPlayers - 1 ? 
                                              table.ActivePlayers[0] :
                                              table.ActivePlayers[table.ActivePlayers.IndexOf(dealer) + 1];
                       
                       var smallBlind = table.ActivePlayers.First(player => player.Button == ButtonTypeNumber.SmallBlind);
                       var nextSmallBlind = smallBlind.IndexNumber == table.MaxPlayers ? 
                                                  table.ActivePlayers[0] : 
                                                  table.ActivePlayers[table.ActivePlayers.IndexOf(smallBlind) + 1];
                       
                       var bigBlind = table.ActivePlayers.First(player => player.Button == ButtonTypeNumber.BigBlind);
                       var nextBigBlind = bigBlind.IndexNumber == table.MaxPlayers ? 
                                                table.ActivePlayers[0] : 
                                                table.ActivePlayers[table.ActivePlayers.IndexOf(bigBlind) + 1];
                       
                       nextDealer.Button = ButtonTypeNumber.Dealer;
                       nextSmallBlind.Button = ButtonTypeNumber.SmallBlind;
                       nextBigBlind.Button = ButtonTypeNumber.BigBlind;
                       
                       table.DealerIndex = nextDealer.IndexNumber;
                       table.SmallBlindIndex = nextSmallBlind.IndexNumber;
                       table.BigBlindIndex = nextBigBlind.IndexNumber;
                    }

                    break;
            }
            _logger.LogInformation($"table {table.Id}: Method SetDealerAndBlinds. Dealer and Blinds are set");
            
            await _hub.Clients.Group(table.Id.ToString()).SendAsync("SetDealerAndBlinds", JsonSerializer.Serialize(_mapper.Map<List<TableDto>>(table)));
            _logger.LogInformation($"table {table.Id}: Method SetDealerAndBlinds. Dealer and Blinds are set. New table is sent to all players. Method ends");
        }

        private async Task DealPocketCards(Table table)
        {
            _logger.LogInformation($"Table {table.Id}: Method DealPocketCards starts");
            foreach (var player in table.ActivePlayers)
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);
            
            await _hub.Clients.Group(table.Id.ToString())
                .SendAsync("DealPocketCards", JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.ActivePlayers)));
            _logger.LogInformation($"Table {table.Id}: Pocket cards are sent to all players. Method ends");
        }

        private async Task StartPreFlopWagering(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering starts");

            await MakeSmallBlindBet(table, _hub);
            await MakeBigBlindBet(table, _hub);
            
            await _hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceiveTable", JsonSerializer.Serialize(table));
            
            do
            {
                SetCurrentPlayer(table);
                
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveCurrentPlayerId", JsonSerializer.Serialize(table.CurrentPlayer.Id));
                _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Current player is set and sent to all players");
                
                _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Waiting for player's action");
                Waiter.WaitForPlayerBet.WaitOne();
                _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Player's action received");
                
                ProcessPlayerAction(table, table.CurrentPlayer);
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
            } 
            while (CheckIfAllBetsAreEqual(table.Players));

            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Players made their choices");
            
            CollectBetsFromTable(table);
            _logger.LogInformation($"Table pot after collecting: {table.Pot}");
            
            await _hub.Clients.All
                .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));

            table.CurrentPlayer = null;

            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering ends");
        }
        
        private async Task DealCommunityCards(Table table, int numberOfCards)
        {
            _logger.LogInformation($"Table {table.Id}: Method DealCommunityCards starts");
            var cardsToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            table.CommunityCards.AddRange(cardsToAdd);
            
            await _hub.Clients.Group(table.Id.ToString()).SendAsync("DealCommunityCards", JsonSerializer.Serialize(table.CommunityCards));
            _logger.LogInformation($"Table {table.Id}: Community cards ({numberOfCards}) are sent to all users. Method ends");
        }

        private async Task StartWagering(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method StartWagering starts");

            var counter = table.ActivePlayers.Count;

            do
            {
                // choose next player to make choice
                SetCurrentPlayer(table);
                // player makes choice
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveCurrentPlayerId", JsonSerializer.Serialize(table.CurrentPlayer.Id)); //receive player's choice
                _logger.LogInformation($"table {table.Id}: Method StartWagering. Current player set and sent to all players");
                
                _logger.LogInformation($"table {table.Id}: Method StartWagering. Waiting for player's action");
                Waiter.WaitForPlayerBet.WaitOne();
                _logger.LogInformation($"table {table.Id}: Method StartWagering. Player's action received");
                
                ProcessPlayerAction(table, table.CurrentPlayer);
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));

                counter--;
            } 
            while (counter > 0 || CheckIfAllBetsAreEqual(table.Players));

            _logger.LogInformation($"table {table.Id}: Method StartWagering. Players made their choices");
            
            CollectBetsFromTable(table);
            _logger.LogInformation($"Table pot after collecting: {table.Pot}");
            
            await _hub.Clients.All
                .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
            
            _logger.LogInformation($"table {table.Id}: Method StartWagering ends");
        }

        private async Task Showdown(Table table)
        {
            var winners = CardsAnalyzer.DefineWinner(table.CommunityCards, table.ActivePlayers);
            table.Winners.AddRange(winners);

            if (winners.Count == 1)
                winners[0].StackMoney += table.Pot;
            else
            {
                var winningAmount = table.Pot / winners.Count;

                foreach (var player in winners)
                    player.StackMoney += winningAmount;
            }

            await _hub.Clients.Group(table.Id.ToString()).SendAsync("ReceiveWinners", table);
        }

        #region PrivateHelpers
        
        private static bool CheckIfAllBetsAreEqual(IReadOnlyCollection<Player> players)
        {
            var amountToCompare = players
                .First(player => player.CurrentAction.ActionType != PlayerActionType.Fold)
                .CurrentAction
                .Amount;

            return players.All(player => 
                player.CurrentAction.ActionType == PlayerActionType.Fold || 
                player.CurrentAction.Amount == amountToCompare);
        }

        private static void SetCurrentPlayer(Table table)
        {
            // if round starts
            if (table.CurrentPlayer == null)
            {
                switch (table.ActivePlayers.Count)
                {
                    case 2:
                        table.CurrentPlayer = table.ActivePlayers.First(player => player.IndexNumber == table.SmallBlindIndex);
                        break;
                    case 3:
                        table.CurrentPlayer = table.ActivePlayers.First(player => player.IndexNumber == table.DealerIndex);
                        break;
                    default:
                        var bigBlind = table.ActivePlayers.First(player => player.IndexNumber == table.BigBlindIndex);
                        table.CurrentPlayer = table.ActivePlayers.IndexOf(bigBlind) == table.ActivePlayers.Count - 1 
                                            ? table.ActivePlayers[0] 
                                            : table.ActivePlayers[table.ActivePlayers.IndexOf(bigBlind) + 1];
                        break;
                }
            }
            else  
                // if round already started and current player is the last one
            if (table.CurrentPlayer.IndexNumber == table.MaxPlayers - 1)
                table.CurrentPlayer = table.ActivePlayers[0];
            else // if current player is not the last pne on table
                table.CurrentPlayer = table.ActivePlayers[table.ActivePlayers.IndexOf(table.CurrentPlayer) + 1];
        }
        
        private static void CollectBetsFromTable(Table table)
        {
            foreach (var player in table.ActivePlayers.Where(player => player.CurrentAction.Amount != null))
            {
                table.Pot += player.CurrentBet;
            }
        }

        private static void ProcessPlayerAction(Table table, Player player)
        {
            switch (player.CurrentAction.ActionType)
            {
                case PlayerActionType.Fold:
                    table.ActivePlayers.Remove(player);
                    break;
                case PlayerActionType.Check:
                    break;
                case PlayerActionType.Bet:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;
                    break;
                case PlayerActionType.Call:
                    player.StackMoney -= table.CurrentMaxBet - player.CurrentBet;
                    player.CurrentBet = table.CurrentMaxBet;
                    break;
                case PlayerActionType.Raise:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount + player.CurrentBet;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task MakeSmallBlindBet(Table table, IHubContext<GameHub> hub)
        {
            var player = table.ActivePlayers.First(player => player.IndexNumber == table.SmallBlindIndex);
            
            var smallBlindAction = new PlayerAction
            {
                PlayerId = player.Id,
                ActionType = PlayerActionType.Bet,
                Amount = table.SmallBlind
            };

            player.CurrentAction = smallBlindAction;
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceivePlayerAction", JsonSerializer.Serialize(smallBlindAction));
            
            ProcessPlayerAction(table, player);
        }
        
        private static async Task MakeBigBlindBet(Table table, IHubContext<GameHub> hub)
        {
            var player = table.ActivePlayers.First(player => player.IndexNumber == table.BigBlindIndex);
            
            var bigBlindAction = new PlayerAction
            {
                PlayerId = player.Id,
                ActionType = PlayerActionType.Raise,
                Amount = table.BigBlind
            };

            player.CurrentAction = bigBlindAction;
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceivePlayerAction", JsonSerializer.Serialize(bigBlindAction));
            
            ProcessPlayerAction(table, player);
        }
        #endregion
    }
}