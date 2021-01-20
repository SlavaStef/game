﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.HandEvaluator;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Table;
using PokerHand.Server.Hubs;
using PokerHand.Server.Hubs.Interfaces;

namespace PokerHand.Server.Helpers
{
    public interface IGameProcessManager
    {
        public Task StartRound(Guid tableId);
    }
    
    public class GameProcessManager : IGameProcessManager
    {
        private readonly List<Table> _allTables;
        private readonly IHubContext<GameHub, IGameHubClient> _hub;
        private readonly ITableService _tableService;
        private readonly IPlayerService _playerService;
        private readonly IMapper _mapper;
        private readonly ILogger<GameHub> _logger;

        public GameProcessManager(
            TablesCollection tablesCollection,
            IHubContext<GameHub, IGameHubClient> hubContext,
            IMapper mapper,
            ILogger<GameHub> logger, 
            ITableService tableService, 
            IPlayerService playerService)
        {
            _allTables = tablesCollection.Tables;
            _hub = hubContext;
            _mapper = mapper;
            _logger = logger;
            _tableService = tableService;
            _playerService = playerService;
        }
        
        public async Task StartRound(Guid tableId)
        {
            _logger.LogInformation("StartRound. Start");
            var table = _allTables.First(t => t.Id == tableId);

            if (table.Players.Count < 2)
            {
                _logger.LogInformation($"StartRound. Table {table.Id}. Waiting for second player");
                await _hub.Clients.Group(table.Id.ToString()).WaitForPlayers();
            }

            while (table.Players.Count < 2)
                Thread.Sleep(1000);

            _logger.LogInformation("StartRound. Round started");

            table.ActivePlayers = table.Players.ToList();
            _logger.LogInformation("StartRound. Active players are set");
            
            while (table.ActivePlayers.Any(p => p.IsReady != true))
                Thread.Sleep(500);
            _logger.LogInformation("StartRound. Start with two players");
            
            await SetDealerAndBlinds(table); // Start chain of game methods
            
            _logger.LogInformation("StartRound. End");

            // Start new round is there is any player
            if (table.Players.Count > 0)
            {
                _logger.LogInformation("StartRound. New round starts");
                RefreshTable(table);
                await StartRound(table.Id);
            }
            
            Thread.CurrentThread.Abort();
        }
                
        private async Task SetDealerAndBlinds(Table table)
        {
            _logger.LogInformation("SetDealerAndBlinds. Start");

            table.CurrentStage = RoundStageType.SetDealerAndBlinds;
            
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

                    table.DealerIndex = table.SmallBlindIndex;
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
            
            await _hub.Clients.Group(table.Id.ToString())
                .SetDealerAndBlinds(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            _logger.LogInformation("SetDealerAndBlinds. Dealer and Blinds are set. New table is sent to all players");
            
            _logger.LogInformation("SetDealerAndBlinds. End");
            
            await DealPocketCards(table);
        }

        private async Task DealPocketCards(Table table)
        {
            _logger.LogInformation("DealPocketCards. Start");

            table.CurrentStage = RoundStageType.DealPocketCards;
            _logger.LogInformation("DealPocketCards. 1");
            foreach (var player in table.ActivePlayers)
            {
                _logger.LogInformation($"DealPocketCards. {player.Id}");
                _logger.LogInformation($"DealPocketCards. {JsonSerializer.Serialize(player)}");
                try
                {
                    player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"DealPocketCards. {e.Message}, {e.StackTrace}");;
                    throw;
                }
                
            }
            _logger.LogInformation("DealPocketCards. 2");    
            await _hub.Clients.Group(table.Id.ToString()).DealPocketCards(JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.ActivePlayers)));
            _logger.LogInformation("DealPocketCards. Pocket cards ara sent to players");
            _logger.LogInformation("DealPocketCards. End");
            
            Thread.Sleep(1000); // wait for cards to be dealed

            if (table.ActivePlayers.Count > 1)
                await StartWagering(table);
            else
                await EndRoundIfFold(table);
        }
        
        private async Task DealCommunityCards(Table table, int numberOfCards)
        {
            _logger.LogInformation("DealCommunityCards. Start");

            _logger.LogInformation($"DealCommunityCards. Before adding stage: {JsonSerializer.Serialize(table)}");
            //table.CurrentStage = RoundStageType.DealCommunityCards;
            _logger.LogInformation("DealCommunityCards. Stage is set");
            
            var cardsToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            
            _logger.LogInformation($"DealCommunityCards. CardsToAdd: {JsonSerializer.Serialize(cardsToAdd)}");
            
            _logger.LogInformation($"DealCommunityCards. Before adding cards: {JsonSerializer.Serialize(table)}");
            table.CommunityCards.AddRange(cardsToAdd);
            _logger.LogInformation($"DealCommunityCards. After adding stage: {JsonSerializer.Serialize(table)}");
            _logger.LogInformation("DealCommunityCards. CommunityCards added to table");
            
            await _hub.Clients.Group(table.Id.ToString()).DealCommunityCards(JsonSerializer.Serialize(cardsToAdd));
            _logger.LogInformation($"DealCommunityCards. Community cards ({numberOfCards}) are sent to all users");
            _logger.LogInformation("DealCommunityCards. End");

            if (table.ActivePlayers.Count > 1)
                await StartWagering(table);
            else
                await EndRoundIfFold(table);
        }

        private async Task StartWagering(Table table)
        {
            _logger.LogInformation("StartWagering. Start");
            
            switch (table.CurrentStage)
            {
                case RoundStageType.DealPocketCards:
                    table.CurrentStage = RoundStageType.WageringPreFlopRound;
                    break;
                case RoundStageType.WageringPreFlopRound:
                    table.CurrentStage = RoundStageType.WageringSecondRound;
                    break;
                case RoundStageType.WageringSecondRound:
                    table.CurrentStage = RoundStageType.WageringThirdRound;
                    break;
                case RoundStageType.WageringThirdRound:
                    table.CurrentStage = RoundStageType.WageringFourthRound;
                    break;
            }

            await MakeBlindBets(table);
            
            var counter = table.ActivePlayers.Count;
            _logger.LogInformation($"StartWagering. Counter: {counter}");
        
            // Players make choices
            do
            {
                if (table.ActivePlayers.Count < 1)
                    EndRoundIfFold(table);
                
                // choose next player to make choice
                await SetCurrentPlayer(table);
        
                _logger.LogInformation("StartWagering. Waiting for player's action");
                table.WaitForPlayerBet.WaitOne();
                _logger.LogInformation("StartWagering. Player's action received");
                
                await ProcessPlayerAction(table, table.CurrentPlayer);
                if (table.ActivePlayers.Count == 1)
                {
                    EndRoundIfFold(table);
                    return;
                }
                _logger.LogInformation("StartWagering. Player's action processed. Ready to sent table to players");
                
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
        
                counter--;
                _logger.LogInformation($"StartWagering. New Table is sent to all players. End of cycle, counter: {counter}");
            } 
            while (table.ActivePlayers.Count > 1 && (counter > 0 || !CheckIfAllBetsAreEqual(table)));
            
            _logger.LogInformation("StartWagering. All players made their choices");
        
            if (table.ActivePlayers.Count == 1)
            {
                _logger.LogInformation("StartWagering. There is one player. EdRoundIfFold");
                EndRoundIfFold(table);
                return;
            }
            
            CollectBetsFromTable(table);
            _logger.LogInformation($"StartWagering. Table pot after collecting: {table.Pot}");
            
            if (table.CurrentMaxBet > 0)
                Thread.Sleep(500);

            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            foreach (var player in table.ActivePlayers)
                player.CurrentBet = 0;

            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveTableStateAtWageringEnd(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            _logger.LogInformation("StartWagering. Final table is sent to clients");
            
            Thread.Sleep(2500);
            
            _logger.LogInformation("StartWagering. End");
        
            // Run next stage
            switch (table.CurrentStage)
            {
                case RoundStageType.WageringPreFlopRound:
                    await DealCommunityCards(table, 3);
                    break;
                case RoundStageType.WageringFourthRound:
                    await Showdown(table);
                    break;
                default:
                    await DealCommunityCards(table, 1);
                    break;
            }
        }
        
        private async Task Showdown(Table table)
        {
            _logger.LogInformation("Showdown. Start");

            table.CurrentStage = RoundStageType.Showdown;

            var isJokerGame = table.Type == TableType.JokerPoker;

            var winners = CardEvaluator.DefineWinners(table.CommunityCards, table.ActivePlayers, isJokerGame);
            _logger.LogInformation("Showdown. Winner(s) defined");
            
            table.Winners = winners.ToList();
            _logger.LogInformation("Showdown. Winner(s) added to table");
            
            if (winners.Count == 1)
                winners[0].StackMoney += table.Pot;
            else
            {
                var winningAmount = table.Pot / winners.Count;
            
                foreach (var player in winners)
                    player.StackMoney += winningAmount;
            }

            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveWinners(JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.Winners)));
            _logger.LogInformation("Showdown. Winners are sent to players");
            
            Thread.Sleep(6000 + (table.ActivePlayers.Count() * 200)); 
            
            _logger.LogInformation("Showdown. End");
        }

        private async Task EndRoundIfFold(Table table)
        {
            _logger.LogInformation("EndRoundIfFold. Start");
            table.Winners.Add(table.ActivePlayers.First());

            _logger.LogInformation($"EndRoundIfFold. Table winners: {JsonSerializer.Serialize(table.Winners)}");
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveWinners(JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.Winners)));
            _logger.LogInformation($"EndRoundIfFold. Sent to client");
            
            Thread.Sleep(6000 + (table.ActivePlayers.Count() * 200));
            
            _logger.LogInformation("EndRoundIfFold. End");
        }

        #region PrivateHelpers
        
        private bool CheckIfAllBetsAreEqual(Table table)
        {
            _logger.LogInformation("CheckIfAllBetsAreEqual. Start");
            _logger.LogInformation(JsonSerializer.Serialize(table.ActivePlayers));
            // var amountToCompare = players
            //     .First(player => player.CurrentAction.ActionType != PlayerActionType.Fold)
            //     .CurrentBet;
            
            _logger.LogInformation($"CheckIfAllBetsAreEqual. Amount to compare: {table.CurrentMaxBet}");

            var result = table.ActivePlayers.All(player => 
                player.CurrentAction.ActionType == PlayerActionType.Fold || 
                player.CurrentBet == table.CurrentMaxBet);
            
            _logger.LogInformation($"CheckIfAllBetsAreEqual. Result: {result}");
            _logger.LogInformation("CheckIfAllBetsAreEqual. End");
            return result;
        }

        private async Task SetCurrentPlayer(Table table)
        {
            _logger.LogInformation("SetCurrentPlayer. Start");

            if (table.CurrentPlayer == null) // if round starts and there is no current player
            {
                table.CurrentPlayer = table.CurrentStage == RoundStageType.WageringPreFlopRound
                    ? GetNextPlayer(table, table.BigBlindIndex)
                    : GetNextPlayer(table, table.DealerIndex);
            }
            else // if round has already started and there is a current player
            {
                table.ActivePlayers = table.ActivePlayers.OrderBy(p => p.IndexNumber).ToList();
                table.CurrentPlayer = GetNextPlayer(table, table.CurrentPlayer.IndexNumber);
            }
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveCurrentPlayerIdInWagering(JsonSerializer.Serialize(table.CurrentPlayer.Id));
            _logger.LogInformation("SetCurrentPlayer. Current player is set and sent to all players");
            
            _logger.LogInformation("SetCurrentPlayer. End");
        }
        
        private void CollectBetsFromTable(Table table)
        {
            foreach (var player in table.ActivePlayers.Where(player => player.CurrentBet != 0))
            {
                table.Pot += player.CurrentBet;
                player.CurrentBet = 0;
            }
        }

        private async Task ProcessPlayerAction(Table table, Player player)
        {
            _logger.LogInformation("ProcessPlayerAction. Start");
            
            switch (player.CurrentAction.ActionType)
            {
                // check if stackMoney >= Amount => is ok
                // else GetFromTotalMoney()
                //      if(isOk)
                case PlayerActionType.Fold:
                    if (table.Type == TableType.DashPoker)
                    {
                        await _tableService.RemovePlayerFromTable(table.Id, player.Id);
                        await _hub.Groups.RemoveFromGroupAsync(player.ConnectionId, table.Id.ToString());
                        
                        await _hub.Clients.Group(table.Id.ToString())
                            .PlayerDisconnected(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
                    }
                    else
                    {
                        if (table.ActivePlayers.Count == 2)
                        {
                            CollectBetsFromTable(table);
                            table.ActivePlayers.Remove(player);
                        }
                        else
                        {
                            table.Pot += player.CurrentBet;
                            table.ActivePlayers.Remove(player);
                        }
                    }
                    break;
                case PlayerActionType.Check:
                    break;
                case PlayerActionType.Bet:
                    _logger.LogInformation("BET. Start");
                    _logger.LogInformation($"BET. player.StackMoney: {player.StackMoney}");
                    _logger.LogInformation($"BET. player.CurrentAction.Amount: {player.CurrentAction.Amount}");
                    _logger.LogInformation($"BET. player.IsAutoTop: {player.IsAutoTop}");
                    // AutoTop
                    if (player.StackMoney < player.CurrentAction.Amount && player.IsAutoTop)
                    {
                        _logger.LogInformation($"AutoTop. Start. player.StackMoney: {player.StackMoney}");
                        var moneyFromTotalMoney = await _playerService.GetStackMoney(player.Id, player.CurrentBuyIn);
                        
                        _logger.LogInformation($"moneyFromTotalMoney: {moneyFromTotalMoney}");
                        if (moneyFromTotalMoney != 0)
                        {
                            _logger.LogInformation($"IF. Start");
                            player.StackMoney += moneyFromTotalMoney;
                            _logger.LogInformation($"IF. End. player.StackMoney: {player.StackMoney}");
                            
                            await _hub.Clients.Client(player.ConnectionId)
                                .ReceivePlayerProfile(JsonSerializer.Serialize(_mapper.Map<PlayerProfileDto>(player)));
                        }
                            
                        _logger.LogInformation("BET. Start");
                    }
                    
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;
                    _logger.LogInformation("BET. End");
                    break;
                case PlayerActionType.Call:
                    // AutoTop
                    if (player.StackMoney < player.CurrentAction.Amount && player.IsAutoTop)
                    {
                        var moneyFromTotalMoney = await _playerService.GetStackMoney(player.Id, player.CurrentBuyIn);

                        if (moneyFromTotalMoney != 0)
                            player.StackMoney += moneyFromTotalMoney;
                        
                        await _hub.Clients.Client(player.ConnectionId)
                            .ReceivePlayerProfile(JsonSerializer.Serialize(_mapper.Map<PlayerProfileDto>(player)));
                    }
                    
                    player.StackMoney -= table.CurrentMaxBet - player.CurrentBet;
                    player.CurrentBet = table.CurrentMaxBet;
                    break;
                case PlayerActionType.Raise:
                    // AutoTop
                    if (player.StackMoney < player.CurrentAction.Amount && player.IsAutoTop)
                    {
                        var moneyFromTotalMoney = await _playerService.GetStackMoney(player.Id, player.CurrentBuyIn);

                        if (moneyFromTotalMoney != 0)
                            player.StackMoney += moneyFromTotalMoney;
                        
                        await _hub.Clients.Client(player.ConnectionId)
                            .ReceivePlayerProfile(JsonSerializer.Serialize(_mapper.Map<PlayerProfileDto>(player)));
                    }
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount + player.CurrentBet;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _logger.LogInformation("ProcessPlayerAction. End");
        }

        private async Task MakeBlindBets(Table table)
        {
            if (table.CurrentStage == RoundStageType.WageringPreFlopRound)
            {
                await MakeSmallBlindBet(table);
                await MakeBigBlindBet(table);
            
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            }
        }
        
        private async Task MakeSmallBlindBet(Table table)
        {
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.SmallBlindIndex);
            
            var smallBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Bet,
                Amount = table.SmallBlind
            };

            player.CurrentAction = smallBlindAction;
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceivePlayerAction(JsonSerializer.Serialize(smallBlindAction));
            _logger.LogInformation("MakeSmallBlindBet. Action is sent to players");
            
            ProcessPlayerAction(table, player);
        }
        
        private async Task MakeBigBlindBet(Table table)
        {
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.BigBlindIndex);
            
            var bigBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Raise,
                Amount = table.BigBlind
            };

            player.CurrentAction = bigBlindAction;
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceivePlayerAction(JsonSerializer.Serialize(bigBlindAction));
            _logger.LogInformation("MakeBigBlindBet. Action is sent to players");
            
            ProcessPlayerAction(table, player);
        }

        private static Player GetNextPlayer(Table table, int currentPlayerIndex)
        {
            return table.ActivePlayers.FirstOrDefault(p => p.IndexNumber > currentPlayerIndex) ?? table.ActivePlayers[0];
        }

        private static void RefreshTable(Table table)
        {
            table.Deck = new Deck(table.Type);

            table.CurrentStage = RoundStageType.None;
            table.ActivePlayers = new List<Player>(table.MaxPlayers);
            table.CommunityCards = new List<Card>(5);
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            table.Pot = 0;
            table.DealerIndex = -1;
            table.SmallBlindIndex = -1;
            table.BigBlindIndex = -1;
            table.Winners = new List<Player>(table.MaxPlayers);
        }
        #endregion
    }
}