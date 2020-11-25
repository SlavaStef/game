using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
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
            var table = _allTables.First(t => t.Id == tableId);
            table.IsInGame = true;
            
            _logger.LogInformation($"table {table.Id}: Round started. Waiting for two players");
            
            while (table.Players.Count < 2)
                System.Threading.Thread.Sleep(1000);
            
            _logger.LogInformation($"table {table.Id}: Game started");

            table.ActivePlayers = table.Players;
            
            await SetDealerAndBlinds(table);
            await DealPocketCards(table);
            await StartPreFlopWagering(table);
            await DealCommonCards(table, 3); // Deal Pre-flop cards
            // await StartWagering(table); // Deal 2-nd wagering
            // await DealCommonCards(table, 1); // Deal Turn card
            // await StartWagering(table); // Deal 3-d wagering
            // await DealCommonCards(table, 1); // Deal River card
            // await StartWagering(table); // Deal 4-th wagering
            // await DefineWinner(table);

            table.IsInGame = false;
        }

        private async Task SetDealerAndBlinds(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method SetDealerAndBlinds starts");

            var currentDealer = table.Players.FirstOrDefault(player => player.Button == ButtonTypeNumber.Dealer)
                ?.IndexNumber;
            var currentSmallBlind = table.Players.FirstOrDefault(player => player.Button == ButtonTypeNumber.SmallBlind)
                ?.IndexNumber;
            var currentBigBlind = table.Players.FirstOrDefault(player => player.Button == ButtonTypeNumber.BigBlind)
                ?.IndexNumber;
            
            switch (table.Players.Count)
            {
                case 1:
                    //TODO: handle this situation
                    break;
                case 2: // if two players, there is no dealer ( smallBlind == dealer )
                    if (currentSmallBlind == null) // blinds are not set yet -> set blinds starting from the first player
                    {
                        table.Players.First(player => player.IndexNumber == 1).Button = ButtonTypeNumber.SmallBlind;
                        table.Players.First(player => player.IndexNumber == 2).Button = ButtonTypeNumber.BigBlind;
                    }
                    else if ((int) currentSmallBlind == 1)
                    {
                        table.Players.First(player => player.IndexNumber == 1).Button = ButtonTypeNumber.BigBlind;
                        table.Players.First(player => player.IndexNumber == 2).Button = ButtonTypeNumber.SmallBlind;
                    }
                    else
                    {
                        table.Players.First(player => player.IndexNumber == 1).Button = ButtonTypeNumber.SmallBlind;
                        table.Players.First(player => player.IndexNumber == 2).Button = ButtonTypeNumber.BigBlind;
                    }
                    break;
                default:
                    //TODO: think over this algorithm
                    table.Players.First(player => player.Button == ButtonTypeNumber.BigBlind).Button =
                        ButtonTypeNumber.SmallBlind;
                    table.Players.First(player => player.Button == ButtonTypeNumber.SmallBlind).Button =
                        ButtonTypeNumber.Dealer;
                    table.Players.First(player => player.Button == ButtonTypeNumber.Dealer).Button =
                        ButtonTypeNumber.BigBlind;
                    break; 
            }
            _logger.LogInformation($"table {table.Id}: Method SetDealerAndBlinds. Dealer and Blinds are set");

            // TODO: change playerDto to player
            await _hub.Clients.Group(table.Id.ToString()).SendAsync("SetDealerAndBlinds", JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.Players)));
            _logger.LogInformation($"table {table.Id}: Method SetDealerAndBlinds. Dealer and Blinds are set. New table is sent to all players. Method ends");
        }
        
        private async Task DealPocketCards(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method DealPoketCards starts");
            
            //TODO: change to active players
            foreach (var player in table.Players)
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);
            
            _logger.LogInformation($"table {table.Id}: Method DealPoketCards. Each player got two cards");

            //TODO: change to active players
            await _hub.Clients.Group(table.Id.ToString()).SendAsync("DealPocketCards", JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.Players)));
            _logger.LogInformation($"table {table.Id}: Method DealPoketCards. New tableDto is sent to all players. Method end");
        }

        private async Task StartPreFlopWagering(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering starts");

            await MakeSmallBlindBet(table, _hub);
            await MakeBigBlindBet(table, _hub);
            
            do
            {
                // choose next player to make choice
                SetCurrentPlayer(table);
                // player makes choice
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveCurrentPlayerId", JsonSerializer.Serialize(table.CurrentPlayer.Id)); //receive player's choice
                _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Current player set and sent to all players");
                _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Waiting for player's action");
                Waiter.WaitForPlayerBet.WaitOne();
                _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Player's action recieved");
                
                ProcessPlayerAction(table, table.CurrentPlayer);
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
            } 
            while (CheckIfAllBetsAreEqual(table.Players));

            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering. Players made their choices");
            
            CollectBetsFromTable(table);
            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering ends");
        }
        
        private async Task DealCommonCards(Table table, int numberOfCards)
        {
            var cardsToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            table.CommunityCards.AddRange(cardsToAdd);
            
            await _hub.Clients.Group(table.Id.ToString()).SendAsync("DealCommunityCards", JsonSerializer.Serialize(table.CommunityCards));
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
                switch (table.Players.Count)
                {
                    case 2:
                        table.CurrentPlayer = table.Players.First(player => player.Button == ButtonTypeNumber.SmallBlind);
                        break;
                    case 3:
                        table.CurrentPlayer = table.Players.First(player => player.Button == ButtonTypeNumber.Dealer);
                        break;
                    default:
                    {
                        var bigBlindIndex = table.Players.First(player => player.Button == ButtonTypeNumber.BigBlind).IndexNumber;
                        table.CurrentPlayer = table.Players.First(player => player.IndexNumber == bigBlindIndex + 1);
                        break;
                    }
                }
            }
            else  
            // if round already started and current player is the last one
            if (table.CurrentPlayer.IndexNumber == table.Players.Count)
                table.CurrentPlayer = table.Players.First(player => player.IndexNumber == 1);
            else // if not the last
            {
                var currentPlayerId = table.CurrentPlayer.IndexNumber;
                table.CurrentPlayer = table.Players.First(player => player.IndexNumber == currentPlayerId + 1);
            }
        }
        
        private static void CollectBetsFromTable(Table table)
        {
            foreach (var player in table.Players.Where(player => player.CurrentAction.Amount != null))
            {
                table.Pot += (int)player.CurrentAction.Amount;
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
                    player.StackMoney -= table.CurrentMaxBet;
                    player.CurrentBet = table.CurrentMaxBet;
                    break;
                case PlayerActionType.Raise:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task MakeSmallBlindBet(Table table, IHubContext<GameHub> hub)
        {
            var smallBlindAction = new PlayerAction
            {
                TableId = table.Id,
                PlayerId = table.Players.First(player => player.Button == ButtonTypeNumber.SmallBlind).Id,
                ActionType = PlayerActionType.Bet,
                Amount = table.SmallBlind
            };

            table.Players.First(player => player.Id == smallBlindAction.PlayerId).CurrentAction = smallBlindAction;
            
            //TODO: Add serialization
            await hub.Clients.Group(table.Id.ToString()).SendAsync("ReceivePlayerAction", JsonSerializer.Serialize(smallBlindAction));
            ProcessPlayerAction(table, table.Players.First(player => player.Id == smallBlindAction.PlayerId));
        }
        
        private static async Task MakeBigBlindBet(Table table, IHubContext<GameHub> hub)
        {
            var bigBlindAction = new PlayerAction
            {
                TableId = table.Id,
                PlayerId = table.Players.First(player => player.Button == ButtonTypeNumber.BigBlind).Id,
                ActionType = PlayerActionType.Raise,
                Amount = table.BigBlind
            };

            table.Players.First(player => player.Id == bigBlindAction.PlayerId).CurrentAction = bigBlindAction;
            
            await hub.Clients.Group(table.Id.ToString()).SendAsync("ReceivePlayerAction", JsonSerializer.Serialize(bigBlindAction));
            ProcessPlayerAction(table, table.Players.First(player => player.Id == bigBlindAction.PlayerId));
        }
        
        #endregion
    }
}