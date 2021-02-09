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
        Task StartRound(Guid tableId);
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
        
        private async Task RunNextStage(Table table)
        {
            switch (table.CurrentStage)
            {
                case RoundStageType.NotStarted:
                    await PrepareForGame(table);
                    break;
                case RoundStageType.PrepareTable:
                    await StartWagering(table);
                    break;
                case RoundStageType.WageringPreFlopRound:
                    await DealCommunityCards(table, 3);
                    await StartWagering(table);
                    break;
                case RoundStageType.DealCommunityCards:
                    break;
                case RoundStageType.WageringSecondRound:
                    await DealCommunityCards(table, 1);
                    await StartWagering(table);
                    break;
                case RoundStageType.WageringThirdRound:
                    await DealCommunityCards(table, 1);
                    await StartWagering(table);
                    break;
                case RoundStageType.WageringFourthRound:
                    await Showdown(table);
                    break;
                case RoundStageType.Showdown:
                    await RefreshTable(table);
                    break;
                case RoundStageType.Refresh:
                    if (table.Type == TableType.SitAndGo && table.Players.Count == 1)
                        _tableService.RemoveTableById(table.Id);
                    if (table.Players.Count > 0)
                        await StartRound(table.Id);
                    break;
            }
        }
        
        public async Task StartRound(Guid tableId)
        {
            _logger.LogInformation("StartRound. Start");
            var table = _allTables.First(t => t.Id == tableId);
            table.CurrentStage = RoundStageType.NotStarted;
            
            WaitForPlayers(table);

            await RunNextStage(table);
            
            _logger.LogInformation("StartRound. End");
        }

        // Preparation
        private async Task PrepareForGame(Table table)
        {
            table.CurrentStage = RoundStageType.PrepareTable;
            
            SetDealerAndBlinds(table);
            DealPocketCards(table);
            
            await _hub.Clients.Group(table.Id.ToString())
                .PrepareForGame(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));

            // Wait for the animation of dealing cards
            Thread.Sleep(1000);

            await RunNextStage(table);
        }
        
        // Game
        private async Task DealCommunityCards(Table table, int numberOfCards)
        {
            _logger.LogInformation("DealCommunityCards. Start");

            //table.CurrentStage = RoundStageType.DealCommunityCards;
            
            var cardsToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            table.CommunityCards.AddRange(cardsToAdd);
            
            await _hub.Clients.Group(table.Id.ToString())
                .DealCommunityCards(JsonSerializer.Serialize(cardsToAdd));
            
            _logger.LogInformation("DealCommunityCards. End");

            if (table.ActivePlayers.Count > 1 && !table.IsEndDueToAllIn)
                await StartWagering(table);
            else
                await Showdown(table);
        }
        
        private async Task StartWagering(Table table)
        {
            _logger.LogInformation("StartWagering. Start");

            ChooseCurrentStage(table);
            if (table.CurrentStage == RoundStageType.WageringPreFlopRound) 
                await MakeBlindBets(table);
            
            // Counter is used to make sure all players acted at least one time
            var actionCounter = table.ActivePlayers.Count;
            
            // Players make choices
            do
            {
                if (table.ActivePlayers.Count < 1)
                {
                    await Showdown(table);
                    return;
                }

                // Choose next player to make choice
                SetCurrentPlayer(table);
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveCurrentPlayerIdInWagering(JsonSerializer.Serialize(table.CurrentPlayer.Id));

                //TODO: Handle player's leaving during WaitOne
                //TODO: Make timer
                table.WaitForPlayerBet.WaitOne();

                await ProcessPlayerAction(table, table.CurrentPlayer);

                // Check if one of two players folded
                if (table.ActivePlayers.Count == 1)
                {
                    CollectBetsFromTable(table);
                    await Showdown(table);
                    return;
                }

                await _hub.Clients.GroupExcept(table.Id.ToString(), table.CurrentPlayer.ConnectionId)
                    .ReceivePlayerAction(JsonSerializer.Serialize(table.CurrentPlayer.CurrentAction));
                
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));

                // if (CheckForEndRoundDueToAllIn(table))
                // {
                //     await DealCommunityCards(table, 5 - table.CommunityCards.Count);
                //     break;
                // }

                actionCounter--;
            } 
            while (table.ActivePlayers.Count > 1 &&
                   (actionCounter > 0 || !CheckIfAllBetsAreEqual(table)));

            // Check for end due to all in
            if (table.ActivePlayers.Count(p => p.StackMoney != 0) <= 1) 
            {
                CollectBetsFromTable(table);

                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveUpdatedPot(JsonSerializer.Serialize(table.Pot));
                
                table.IsEndDueToAllIn = true;
                await DealCommunityCards(table, 5 - table.CommunityCards.Count);
                return;
            }

            // If at least one action was taken
            if (table.CurrentMaxBet > 0)
            {
                // Wait for animation
                Thread.Sleep(1000);
                
                CollectBetsFromTable(table);

                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveUpdatedPot(JsonSerializer.Serialize(table.Pot));
                
                table.CurrentMaxBet = 0;
                
                // Wait for animation
                Thread.Sleep(500);
            }
            
            table.CurrentPlayer = null;

            if (table.ActivePlayers.Count == 1)
            {
                _logger.LogInformation("StartWagering. There is one player. EdRoundIfFold");
                await Showdown(table);
                return;
            }
            
            //TODO: Check if this method is required. Maybe ReceiveUpdatedPot is enough?
            // await _hub.Clients.Group(table.Id.ToString())
            //     .ReceiveTableStateAtWageringEnd(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            
            Thread.Sleep(2500);

            await RunNextStage(table);
        }

        // End
        private async Task Showdown(Table table)
        {
            _logger.LogInformation("Showdown. Start");

            table.CurrentStage = RoundStageType.Showdown;
            
            if (table.ActivePlayers.Count == 1)
            {
                await EndRoundWithOnePlayer(table);
                return;
            }

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
            
            Thread.Sleep(5000 + (table.ActivePlayers.Count * 200)); 
            
            _logger.LogInformation("Showdown. End");

            await RunNextStage(table);
        }

        private async Task EndRoundWithOnePlayer(Table table)
        {
            _logger.LogInformation("EndRoundNow. Start");
            table.Winners.Add(table.ActivePlayers.First());
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveWinners(JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.Winners)));

            Thread.Sleep(6000 + (table.ActivePlayers.Count * 200));
            
            _logger.LogInformation("EndRoundNow. End");

            await RunNextStage(table);
        }
        
        // CleanUp
        private async Task RefreshTable(Table table)
        {
            table.CurrentStage = RoundStageType.Refresh;
            
            table.Deck = new Deck(table.Type);

            table.CommunityCards = new List<Card>(5);
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            table.Pot = 0;
            // table.DealerIndex = -1;
            // table.SmallBlindIndex = -1;
            // table.BigBlindIndex = -1;
            table.Winners = new List<Player>(table.MaxPlayers);
            table.IsEndDueToAllIn = false;

            // Manage Sit&Go table state
            if (table.Type == TableType.SitAndGo)
            {
                _logger.LogInformation("RefreshTable. Managing Sit&Go");
                table.SmallBlind *= 2;
                table.BigBlind *= 2;
                
                TableDto newTableDto = null;

                foreach (var player in table.ActivePlayers.Where(p => p.StackMoney == 0))
                {
                    // Define player's place
                    var playerPlace = table.Players.Count;
                    
                    _logger.LogInformation($"Before RemoveFromTable: active players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");
                    _logger.LogInformation($"Before RemoveFromTable: active players: {table.ActivePlayers.Count} : {JsonSerializer.Serialize(table.ActivePlayers)}");
                    newTableDto = await _tableService.RemovePlayerFromTable(table.Id, player.Id);
                    _logger.LogInformation($"After RemoveFromTable: active players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");
                    _logger.LogInformation($"After RemoveFromTable: active players: {table.ActivePlayers.Count} : {JsonSerializer.Serialize(table.ActivePlayers)}");
                    
                    await _hub.Groups
                        .RemoveFromGroupAsync(player.ConnectionId, table.Id.ToString());
                    await _hub.Clients.Client(player.ConnectionId)
                        .EndSitAndGoGame(playerPlace.ToString());
                }

                _logger.LogInformation($"RefreshTable. newTableDto: {JsonSerializer.Serialize(newTableDto)}");
                if (newTableDto != null)
                    await _hub.Clients.Group(table.Id.ToString())
                        .ReceiveTableState(JsonSerializer.Serialize(newTableDto));
                _logger.LogInformation("RefreshTable. End managing Sit&Go");
            }

            // If autoTop is Off and stackMoney == 0
            foreach (var player in table.ActivePlayers.Where(p => p.StackMoney == 0))
            {
                await _hub.Clients.Client(player.ConnectionId)
                    .OnLackOfStackMoney();
            }
            
            table.ActivePlayers = new List<Player>(table.MaxPlayers);
            
            Thread.Sleep(5000);
            
            await _hub.Clients.Group(table.Id.ToString())
                .OnGameEnd();

            await RunNextStage(table);
        }
        
        #region PrivateHelpers
        
        private void WaitForPlayers(Table table)
        {
            _logger.LogInformation("StartRound. Waiting for players");
            while (table.Players.Count(p => p.StackMoney > 0) < table.MinPlayersToStart)
                Thread.Sleep(1000);

            _logger.LogInformation("StartRound. Adding players to activePlayers");
            _logger.LogInformation($"StartRound. table before adding: {JsonSerializer.Serialize(table)}");
            foreach (var player in table.Players.Where(p => p.StackMoney > 0))
                table.ActivePlayers.Add(player);

            _logger.LogInformation($"StartRound. table after adding: {JsonSerializer.Serialize(table)}");
            while (table.ActivePlayers.Any(p => p.IsReady != true))
                Thread.Sleep(500);
            _logger.LogInformation("StartRound. Game starts");
        }

        private void SetDealerAndBlinds(Table table)
        {
            _logger.LogInformation("SetDealerAndBlinds. Start");
            
            switch (table.ActivePlayers.Count)
                {
                    // If 2 players -> Dealer == SmallBlind
                    case 2:
                        if (table.DealerIndex < 0 && table.SmallBlindIndex < 0 && table.BigBlindIndex < 0)
                        {
                            table.SmallBlindIndex = table.ActivePlayers
                                .First(p => p.StackMoney > 0).IndexNumber;
                        
                            table.BigBlindIndex = table.ActivePlayers
                                .First(p => p.IndexNumber > table.SmallBlindIndex && p.StackMoney > 0).IndexNumber;
                        }
                        else
                        {
                            // Set SmallBlind
                            var possibleSmallBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.SmallBlindIndex && p.StackMoney > 0);

                            if (possibleSmallBlind == null)
                                table.SmallBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.SmallBlindIndex = possibleSmallBlind.IndexNumber;
                        
                            // Set BigBlind
                            var possibleBigBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.SmallBlindIndex && p.StackMoney > 0);

                            if (possibleBigBlind == null)
                                table.BigBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.BigBlindIndex = possibleBigBlind.IndexNumber;
                        }
                        
                        table.DealerIndex = table.SmallBlindIndex;
                        
                        break;
                    default:
                        // Possible case for Sit and go
                        if (table.DealerIndex < 0 && table.SmallBlindIndex < 0 && table.BigBlindIndex < 0)
                        {
                            table.DealerIndex = 0;
                            table.SmallBlindIndex = 1;
                            table.BigBlindIndex = 2;
                        }
                        else
                        {
                            // Set dealer
                            var possibleDealer = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.DealerIndex && p.StackMoney > 0);

                            if (possibleDealer == null)
                                table.SmallBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.DealerIndex = possibleDealer.IndexNumber;
                            
                            // Set SmallBlind
                            var possibleSmallBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.DealerIndex && p.StackMoney > 0);

                            if (possibleSmallBlind == null)
                                table.SmallBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.SmallBlindIndex = possibleSmallBlind.IndexNumber;
                            
                            // Set BigBlind
                            var possibleBigBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.SmallBlindIndex && p.StackMoney > 0);

                            if (possibleBigBlind == null)
                                table.BigBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.BigBlindIndex = possibleBigBlind.IndexNumber;
                        }
                        break;
                }
            
            _logger.LogInformation("SetDealerAndBlinds. End");
        }

        private void DealPocketCards(Table table)
        {
            _logger.LogInformation("DealPocketCards. Start");
            
            foreach (var player in table.ActivePlayers)
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);

            _logger.LogInformation("DealPocketCards. End");
        }
        
        private bool CheckIfAllBetsAreEqual(Table table)
        {
            var result = table
                .ActivePlayers
                .All(player => player.CurrentAction.ActionType == PlayerActionType.Fold || 
                               player.CurrentBet == table.CurrentMaxBet);
            return result;
        }

        private void SetCurrentPlayer(Table table)
        {
            // if round starts and there is no current player
            if (table.CurrentPlayer == null)
            {
                table.CurrentPlayer = table.CurrentStage == RoundStageType.WageringPreFlopRound
                    ? GetNextPlayer(table, table.BigBlindIndex)
                    : GetNextPlayer(table, table.DealerIndex);
            }
            // if round has already started and there is a current player
            else
            {
                table.ActivePlayers = table.ActivePlayers.OrderBy(p => p.IndexNumber).ToList();
                table.CurrentPlayer = GetNextPlayer(table, table.CurrentPlayer.IndexNumber);
            }
            
            static Player GetNextPlayer(Table table, int currentPlayerIndex) =>
                table.ActivePlayers
                    .FirstOrDefault(p => p.IndexNumber > currentPlayerIndex && p.StackMoney > 0) ?? table.ActivePlayers[0];
        }
        
        private static void ChooseCurrentStage(Table table)
        {
            table.CurrentStage = table.CurrentStage switch
            {
                RoundStageType.PrepareTable => RoundStageType.WageringPreFlopRound,
                RoundStageType.WageringPreFlopRound => RoundStageType.WageringSecondRound,
                RoundStageType.WageringSecondRound => RoundStageType.WageringThirdRound,
                RoundStageType.WageringThirdRound => RoundStageType.WageringFourthRound,
                _ => table.CurrentStage
            };
        }
        
        private static void CollectBetsFromTable(Table table)
        {
            foreach (var player in table.ActivePlayers.Where(player => player.CurrentBet != 0))
            {
                table.Pot += player.CurrentBet;
                player.CurrentBet = 0;
            }
        }

        private async Task ProcessPlayerAction(Table table, Player player)
        {
            switch (player.CurrentAction.ActionType)
            {
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
                        table.Pot += player.CurrentBet;
                        table.ActivePlayers.Remove(player);
                    }
                    break;
                
                // Check - If no one has yet opened the betting round (equivalent to betting zero)
                case PlayerActionType.Check:
                    break;
                
                // Bet - The opening bet of a betting round
                case PlayerActionType.Bet:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;
                    _logger.LogInformation("Bet. End");
                    break;
                
                // Call - To match a bet or raise
                case PlayerActionType.Call:
                    player.StackMoney -= table.CurrentMaxBet - player.CurrentBet;
                    player.CurrentBet = table.CurrentMaxBet;
                    break;
                
                // Raise - To increase the size of an existing bet in the same betting round
                case PlayerActionType.Raise:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount + player.CurrentBet;
                    _logger.LogInformation($"Raise. player: {JsonSerializer.Serialize(player)}");
                    _logger.LogInformation($"Raise. table: {JsonSerializer.Serialize(table)}");
                    _logger.LogInformation("Raise. End");
                    break;
                
                case PlayerActionType.AllIn:
                    player.CurrentBet = player.StackMoney;
                    player.StackMoney = 0;

                    if (table.CurrentMaxBet < player.CurrentBet)
                        table.CurrentMaxBet = player.CurrentBet;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task MakeBlindBets(Table table)
        {
            await MakeSmallBlindBet(table);
            await MakeBigBlindBet(table);
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
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

            if (player.StackMoney < table.SmallBlind)
            {
                smallBlindAction.ActionType = PlayerActionType.AllIn;
                smallBlindAction.Amount = player.StackMoney;
            }  

            player.CurrentAction = smallBlindAction;
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceivePlayerAction(JsonSerializer.Serialize(smallBlindAction));
            
            await ProcessPlayerAction(table, player);
        }
        
        private async Task MakeBigBlindBet(Table table)
        {
            _logger.LogInformation("MakeBigBlindBet. Start");
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.BigBlindIndex);
            
            var bigBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Raise,
                Amount = table.BigBlind
            };
            
            _logger.LogInformation($"MakeBigBlindBet. table: {JsonSerializer.Serialize(table)}");
            _logger.LogInformation($"MakeBigBlindBet. player: {JsonSerializer.Serialize(player)}");
            
            _logger.LogInformation($"MakeBigBlindBet. player.StackMoney: {JsonSerializer.Serialize(player.StackMoney)}");
            _logger.LogInformation($"MakeBigBlindBet. table.BigBlind: {JsonSerializer.Serialize(table.BigBlind)}");
            _logger.LogInformation($"MakeBigBlindBet. player.StackMoney < table.BigBlind: {player.StackMoney < table.BigBlind}");
            if (player.StackMoney < table.BigBlind)
            {
                bigBlindAction.ActionType = PlayerActionType.AllIn;
                bigBlindAction.Amount = player.StackMoney;
            }  

            player.CurrentAction = bigBlindAction;
            
            _logger.LogInformation($"MakeBigBlindBet. player.CurrentAction: {JsonSerializer.Serialize(player.CurrentAction)}");
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceivePlayerAction(JsonSerializer.Serialize(bigBlindAction));
            
            _logger.LogInformation("MakeBigBlindBet. End");
            await ProcessPlayerAction(table, player);
        }

        private async Task AutoTopStackMoney(Table table)
        {
            foreach (var player in table.Players)
            {
                if (player.StackMoney == 0 && player.IsAutoTop && player.TotalMoney >= player.CurrentBuyIn)
                {
                    var isGotMoneyFromTotalMoney = await _playerService.GetFromTotalMoney(player.Id, player.CurrentBuyIn);
                    
                    if (isGotMoneyFromTotalMoney)
                    {
                        player.StackMoney += player.CurrentBuyIn;
                            
                        await _hub.Clients.Client(player.ConnectionId)
                            .ReceivePlayerProfile(JsonSerializer.Serialize(_mapper.Map<PlayerProfileDto>(player)));
                    }
                }
            }
        }
        #endregion
    }
}