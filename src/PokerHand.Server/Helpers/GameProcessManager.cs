using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.CardEvaluator;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Table;
using PokerHand.Server.Hubs;
using PokerHand.Server.Hubs.Interfaces;
using static PokerHand.Common.Helpers.Table.TableType;

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
        
        public async Task StartRound(Guid tableId)
        {
            _logger.LogInformation("StartRound. Start");
            var table = _allTables.First(t => t.Id == tableId);
            table.CurrentStage = RoundStageType.NotStarted;

            _logger.LogInformation($"Start round. Before autotop");
            await AutoTopStackMoney(table);
            _logger.LogInformation($"Start round. After autotop");
            
            WaitForPlayers(table);

            await RunNextStage(table);
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
                    if (table.Type == SitAndGo && table.Players.Count == 1)
                        _tableService.RemoveTableById(table.Id);
                    if (table.Players.Count > 0)
                        await StartRound(table.Id);
                    break;
            }
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

            await Task.Delay(numberOfCards * 300);
            
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
            var actionCounter = table.ActivePlayers.Count(p => p.StackMoney > 0);
            
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

                await _hub.Clients.GroupExcept(table.Id.ToString(), table.CurrentPlayer.ConnectionId)
                    .ReceivePlayerAction(JsonSerializer.Serialize(table.CurrentPlayer.CurrentAction));
                
                // Check if one of two players folded
                if (table.ActivePlayers.Count == 1)
                {
                    await Showdown(table);
                    return;
                }
                
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));

                // if (CheckForEndRoundDueToAllIn(table))
                // {
                //     await DealCommunityCards(table, 5 - table.CommunityCards.Count);
                //     break;
                // }

                actionCounter--;
            } 
            while (!CheckForWageringEnd(table, actionCounter));

            await Task.Delay(1000);

            foreach (var player in table.ActivePlayers)
                player.CurrentBet = 0;
            
            // Check for end due to all in
            if (table.ActivePlayers.Count(p => p.StackMoney is not 0) <= 1) 
            {
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveUpdatedPot(JsonSerializer.Serialize(table.Pot));

                await Task.Delay(table.ActivePlayers.Count * 300);

                table.IsEndDueToAllIn = true;
                await DealCommunityCards(table, 5 - table.CommunityCards.Count);
                return;
            }

            // If at least one action was taken
            if (table.CurrentMaxBet > 0)
            {
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveUpdatedPot(JsonSerializer.Serialize(table.Pot));
                
                await Task.Delay(table.ActivePlayers.Count * 300);

                table.CurrentMaxBet = 0;
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
            
            var isJokerGame = table.Type is JokerPoker;

            var finalSidePots = CalculateFinalSidePots(table, isJokerGame);

            GiveMoneyToWinners(finalSidePots);

            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveWinners(JsonSerializer.Serialize(_mapper.Map<List<SidePotDto>>(finalSidePots)));
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            
            Thread.Sleep(5000 + (table.ActivePlayers.Count * 500)); 
           
            _logger.LogInformation("Showdown. End");
            await RunNextStage(table);
        }

        // CleanUp
        private async Task RefreshTable(Table table)
        {
            table.CurrentStage = RoundStageType.Refresh;
            
            table.Deck = new Deck(table.Type);
            table.Pot = new Pot(table);

            table.CommunityCards = new List<Card>(5);
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            table.Winners = new List<Player>(table.MaxPlayers);
            table.IsEndDueToAllIn = false;

            // Manage Sit&Go table state
            if (table.Type is SitAndGo)
            {
                _logger.LogInformation("RefreshTable. Managing Sit&Go");
                table.SmallBlind *= 2;
                table.BigBlind *= 2;
                
                await RemovePlayersWithEmptyStackMoney(table);

                // Handle winner
                if (table.ActivePlayers.Count is 1)
                {
                    var firstPlacePrize = TableOptions.Tables[table.Title.ToString()]["FirstPlacePrize"];
                    _logger.LogInformation(
                        $"RemovePlayersWithEmptyStackMoney: firstPlacePrize: {firstPlacePrize}");
                    await _playerService.AddTotalMoney(table.ActivePlayers[0].Id, firstPlacePrize);
                    
                    await _hub.Clients.Client(table.ActivePlayers[0].ConnectionId)
                        .EndSitAndGoGame("1");
                }
                
                await _hub.Clients.Group(table.Id.ToString())
                        .ReceiveTableState(JsonSerializer.Serialize(table));

                table.MinPlayersToStart = table.Players.Count;
                _logger.LogInformation("RefreshTable. End managing Sit&Go");
            }

            table.ActivePlayers = new List<Player>(table.MaxPlayers);
            
            Thread.Sleep(5000);
            
            await _hub.Clients.Group(table.Id.ToString())
                .OnGameEnd();

            await RunNextStage(table);
        }

        #region PrivateHelpers
        
        // StartRound helpers
        private async Task AutoTopStackMoney(Table table)
        {
            _logger.LogInformation("AutoTop. Start");
            var minBuyIn = TableOptions.Tables[table.Title.ToString()]["MinBuyIn"];

            foreach (var player in table.Players.Where(player => player.StackMoney is 0 && player.IsAutoTop && player.TotalMoney >= minBuyIn))
            {
                _logger.LogInformation($"AutoTop. Player: {JsonSerializer.Serialize(player)}");
                var wasEnoughMoney = await _playerService.GetFromTotalMoney(player.Id, minBuyIn);
                _logger.LogInformation($"AutoTop. wasEnoughMoney: {wasEnoughMoney}");
                if (wasEnoughMoney)
                {
                    _logger.LogInformation($"AutoTop. old player's stackMoney: {player.StackMoney}");
                    player.StackMoney += minBuyIn;
                            
                    _logger.LogInformation($"AutoTop. new player's stackMoney: {player.StackMoney}");
                    await _hub.Clients.Client(player.ConnectionId)
                        .ReceivePlayerDto(JsonSerializer.Serialize(_mapper.Map<PlayerDto>(player)));
                    _logger.LogInformation("AutoTop. Player profile is sent");
                }
            }
            
            _logger.LogInformation("AutoTop. End");
            
            foreach (var player in table.ActivePlayers.Where(p => p.StackMoney == 0))
                await _hub.Clients.Client(player.ConnectionId)
                    .OnLackOfStackMoney();
            
            _logger.LogInformation("AutoTop. OnLackOfStackMoney is sent");
        }
        
        private void WaitForPlayers(Table table)
        {
            try
            {
                _logger.LogInformation("StartRound. Waiting for players");
                _logger.LogInformation($"StartRound. MinPlayersToStart: {table.MinPlayersToStart}");
                while (table.Players.Count(p => p.StackMoney > 0) < table.MinPlayersToStart)
                    Thread.Sleep(1000);
            
                foreach (var player in table.Players.Where(p => p.StackMoney > 0))
                {
                    table.ActivePlayers.Add(player);
                    table.Pot.Bets.Add(player.Id, 0);
                }

                while (table.ActivePlayers.Any(p => p.IsReady is not true))
                    Thread.Sleep(500);
                _logger.LogInformation("StartRound. Game starts");
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        // PrepareForGame helpers
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

                            if (possibleSmallBlind is null)
                                table.SmallBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.SmallBlindIndex = possibleSmallBlind.IndexNumber;
                        
                            // Set BigBlind
                            var possibleBigBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.SmallBlindIndex && p.StackMoney > 0);

                            if (possibleBigBlind is null)
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

                            if (possibleDealer is null)
                                table.SmallBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.DealerIndex = possibleDealer.IndexNumber;
                            
                            // Set SmallBlind
                            var possibleSmallBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.DealerIndex && p.StackMoney > 0);

                            if (possibleSmallBlind is null)
                                table.SmallBlindIndex = table.ActivePlayers
                                    .First(p => p.StackMoney > 0).IndexNumber;
                            else
                                table.SmallBlindIndex = possibleSmallBlind.IndexNumber;
                            
                            // Set BigBlind
                            var possibleBigBlind = table.ActivePlayers
                                .FirstOrDefault(p => p.IndexNumber > table.SmallBlindIndex && p.StackMoney > 0);

                            if (possibleBigBlind is null)
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
        
        // StartWagering helpers
        private bool CheckForWageringEnd(Table table, int actionCounter)
        {
            if (table.ActivePlayers.Count < 2)
                return true;
            
            if (actionCounter > 0)
                return false;

            if (actionCounter is 0 && CheckIfAllBetsAreEqual(table))
                return true;

            // Check if all players acted AllIn
            if (table.ActivePlayers.All(p => p.StackMoney is 0))
                return true;

            // Check if some players acted AllIn and other acted Call
            foreach (var player in table.ActivePlayers.Where(p => p.StackMoney is not 0))
            {
                if (player.CurrentBet != table.CurrentMaxBet)
                    return false;
            }

            return true;
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
            if (table.CurrentPlayer is null)
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
                    .FirstOrDefault(p => p.IndexNumber > currentPlayerIndex && p.StackMoney > 0) 
                ?? table.ActivePlayers.First(p => p.StackMoney > 0);
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
        
        private async Task ProcessPlayerAction(Table table, Player player)
        {
            try
            {
                switch (player.CurrentAction.ActionType)
            {
                case PlayerActionType.Fold:
                    if (table.Type == DashPoker)
                    {
                        await _tableService.RemovePlayerFromTable(table.Id, player.Id);
                        await _hub.Groups.RemoveFromGroupAsync(player.ConnectionId, table.Id.ToString());

                        await _hub.Clients.Group(table.Id.ToString())
                            .PlayerDisconnected(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
                    }
                    else
                    {
                        table.ActivePlayers.Remove(player);
                        _logger.LogInformation("Player was removed from activePlayers");
                    }
                    break;
                
                // Check - If no one has yet opened the betting round (equivalent to betting zero)
                case PlayerActionType.Check:
                    break;
                
                // Bet - The opening bet of a betting round
                case PlayerActionType.Bet:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;

                    table.Pot.TotalAmount += (int)player.CurrentAction.Amount;
                    table.Pot.Bets[player.Id] += (int) player.CurrentAction.Amount;
                    break;
                
                // Call - To match a bet or raise
                case PlayerActionType.Call:
                    var callAmount = table.CurrentMaxBet - player.CurrentBet;
                    player.StackMoney -= callAmount;
                    player.CurrentBet = table.CurrentMaxBet;

                    table.Pot.TotalAmount += callAmount;
                    table.Pot.Bets[player.Id] += callAmount;
                    break;
                
                // Raise - To increase the size of an existing bet in the same betting round
                case PlayerActionType.Raise:
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount + player.CurrentBet;

                    table.Pot.TotalAmount += (int) player.CurrentAction.Amount;
                    table.Pot.Bets[player.Id] += (int) player.CurrentAction.Amount;
                    break;
                
                case PlayerActionType.AllIn:
                    var allInAmount = player.StackMoney;
                    
                    player.CurrentBet += allInAmount;
                    player.StackMoney = 0;

                    if (table.CurrentMaxBet < player.CurrentBet)
                        table.CurrentMaxBet = player.CurrentBet;

                    table.Pot.TotalAmount += allInAmount;
                    table.Pot.Bets[player.Id] += allInAmount;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
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

        // Showdown helpers 
        private List<SidePot> CreateSidePots(Table table)
        {
            var bets = table.Pot.Bets;
            _logger.LogInformation($"CreateSidePots. Bets: {JsonSerializer.Serialize(bets)}");
            
            var finalSidePotsList = new List<SidePot>();
            
            while (bets.Any(b => b.Value > 0))
            {
                var sidePot = new SidePot();
                
                var minBet = bets
                    .Where(b => b.Value > 0)
                    .Select(bet => bet.Value)
                    .Min();
                    
                _logger.LogInformation($"CreateSidePots. minBet: {JsonSerializer.Serialize(minBet)}");

                // Find all players with bet equal or greater than minBet => add them to current sidePot
                foreach (var (playerId, bet) in bets.Where(b => b.Value >= minBet))
                {
                    _logger.LogInformation($"CreateSidePots. playerId: {JsonSerializer.Serialize(playerId)}");
                    _logger.LogInformation($"CreateSidePots. table: {JsonSerializer.Serialize(table)}");

                    var player = table
                        .ActivePlayers
                        .FirstOrDefault(p => p.Id == playerId);

                    if (player is not null)
                        sidePot.Players.Add(player);

                    sidePot.TotalAmount += minBet;
                    bets[playerId] -= minBet;
                    _logger.LogInformation($"CreateSidePots. player: {JsonSerializer.Serialize(player)}");
                }

                sidePot.Type = finalSidePotsList.Count == 0
                    ? SidePotType.Main
                    : SidePotType.Side;
                
                finalSidePotsList.Add(sidePot);
                _logger.LogInformation($"CreateSidePots. new SidePot: {JsonSerializer.Serialize(sidePot)}");
            }

            _logger.LogInformation($"CreateSidePots. finalSidePotsList: {JsonSerializer.Serialize(finalSidePotsList)}");
            return finalSidePotsList;
        }
        
        private List<SidePot> GetSidePotsWithWinners(List<SidePot> sidePots, List<Player> players)
        {
            _logger.LogInformation("GetSidePotsWithWinners. Start");
            foreach (var sidePot in sidePots)
            {
                var maxHandValue = sidePot.Players
                    .Max(player => player.HandValue);
                
                sidePot.Winners = sidePot.Players
                    .Where(p => p.HandValue == maxHandValue)
                    .ToList();
                
                sidePot.WinningAmountPerPlayer = sidePot.TotalAmount / sidePot.Winners.Count;
            }

            _logger.LogInformation($"GetSidePotsWithWinners. SidePots: {JsonSerializer.Serialize(sidePots)}");
            return sidePots;
        }
        
        private List<SidePot> CalculateFinalSidePots(Table table, bool isJokerGame)
        {
            var finalSidePots = new List<SidePot>();
            
            switch (table.ActivePlayers.Count)
            {
                case 1:
                    finalSidePots.Add(new SidePot()
                    {
                        Type = SidePotType.Main,
                        WinningAmountPerPlayer = table.Pot.TotalAmount,
                        Winners = new List<Player>() {table.ActivePlayers.First()}
                    });
                    break;
                default:
                    _logger.LogInformation("Showdown. Getting playersWithEvaluatedHands");
                    // 1. Evaluate players' cards => get a list of players with HandValue
                    var playersWithEvaluatedHands =
                        CardEvaluator.EvaluatePlayersHands(table.CommunityCards, table.ActivePlayers, isJokerGame);
                    _logger.LogInformation($"Showdown. Players: {JsonSerializer.Serialize(playersWithEvaluatedHands)}");

                    _logger.LogInformation("Showdown. Getting sidePots");
                    // 2. Get a list of sidePots with Type, Players, AmountOfOneBet, TotalAmount
                    var sidePots = CreateSidePots(table);
                    _logger.LogInformation($"Showdown. SidePots: {JsonSerializer.Serialize(sidePots)}");

                    _logger.LogInformation("Showdown. Getting final sidePots");
                    // 3.Get final sidePots
                    finalSidePots = GetSidePotsWithWinners(sidePots, playersWithEvaluatedHands);
                    _logger.LogInformation($"Showdown. SidePots: {JsonSerializer.Serialize(finalSidePots)}");

                    break;
            }

            return finalSidePots;
        }
        
        private void GiveMoneyToWinners(List<SidePot> finalSidePots)
        {
            foreach (var pot in finalSidePots)
            {
                foreach (var player in pot.Winners)
                {
                    player.StackMoney += pot.WinningAmountPerPlayer;
                }
            }
        }
        
        // CleanUp helpers
        private async Task RemovePlayersWithEmptyStackMoney(Table table)
        {
            for (var index = table.ActivePlayers.Count; index < 0; index--)
            {
                if (table.ActivePlayers[index].StackMoney is not 0)
                    continue;
                
                var playerPlace = table.ActivePlayers.Count;

                _logger.LogInformation($"RemovePlayersWithEmptyStackMoney: players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");
                _logger.LogInformation($"RemovePlayersWithEmptyStackMoney: active players: {table.ActivePlayers.Count} : {JsonSerializer.Serialize(table.ActivePlayers)}");
                
                await _tableService.RemovePlayerFromTable(table.Id, table.ActivePlayers[index].Id);
               
                _logger.LogInformation($"RemovePlayersWithEmptyStackMoney: players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");
                _logger.LogInformation($"RemovePlayersWithEmptyStackMoney: active players: {table.ActivePlayers.Count} : {JsonSerializer.Serialize(table.ActivePlayers)}");

                if (playerPlace is 2)
                {
                    var secondPlacePrize = TableOptions.Tables[table.Title.ToString()]["SecondPlacePrize"];
                    _logger.LogInformation($"RemovePlayersWithEmptyStackMoney: secondPlacePrize: {secondPlacePrize}");
                    await _playerService.AddTotalMoney(table.ActivePlayers[index].Id, secondPlacePrize);
                }
                
                await _hub.Groups
                    .RemoveFromGroupAsync(table.ActivePlayers[index].ConnectionId, table.Id.ToString());
                
                _logger.LogInformation($"RemovePlayersWithEmptyStackMoney: Place: {playerPlace}");
                await _hub.Clients.Client(table.ActivePlayers[index].ConnectionId)
                    .EndSitAndGoGame(playerPlace.ToString());
            }
        }

        #endregion
    }
}