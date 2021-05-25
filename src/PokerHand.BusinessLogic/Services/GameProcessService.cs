﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Pot;
using PokerHand.Common.Helpers.Table;
using Serilog;

namespace PokerHand.BusinessLogic.Services
{
    public class GameProcessService : IGameProcessService
    {
        private readonly ITablesOnline _allTables;
        private readonly IDeckService _deckService;
        private readonly IPlayerService _playerService;
        private readonly ITableService _tableService;
        private readonly IBotService _botService;
        private readonly ICardEvaluationService _cardEvaluationService;
        private readonly IMoneyBoxService _moneyBoxService;

        private readonly IMapper _mapper;

        private const bool isGameWithBots = true;
        private IConfiguration Configuration { get; }

        #region Events

        public event Action<Table> OnPrepareForGame;
        public event Action<Table, List<Card>> OnDealCommunityCards;
        public event Action<Table, string> ReceiveWinners;
        public event Action<Table, string> ReceiveUpdatedPot;
        public event Action<Player> ReceivePlayerDto;
        public event Action<Table, PlayerAction> ReceivePlayerAction;
        public event Action<Table, string> ReceiveCurrentPlayerIdInWagering;
        public event Action<Player> OnLackOfStackMoney;
        public event Action<Table> ReceiveTableState;
        public event Action<Table, string> BigBlindBetEvent;
        public event Action<Table, string> SmallBlindBetEvent;
        public event Action<Table, string> NewPlayerBetEvent;
        public event Action<Table> EndSitAndGoGameFirstPlace;
        public event Action<Player, string> EndSitAndGoGame;
        public event Action<string, string> RemoveFromGroupAsync;
        public event Action<Table> OnGameEnd;
        public event Action<string, string> PlayerDisconnected;

        #endregion

        public GameProcessService(
            IMapper mapper,
            IDeckService deckService,
            IPlayerService playerService,
            ITableService tableService,
            IBotService botService,
            ICardEvaluationService cardEvaluationService,
            ITablesOnline allTables, 
            IConfiguration configuration, 
            IMoneyBoxService moneyBoxService)
        {
            _mapper = mapper;
            _deckService = deckService;
            _playerService = playerService;
            _tableService = tableService;
            _botService = botService;
            _cardEvaluationService = cardEvaluationService;
            _allTables = allTables;
            Configuration = configuration;
            _moneyBoxService = moneyBoxService;
        }

        public async Task StartRound(Guid tableId)
        {
            try
            {
                Log.Information("StartRound. Start");
                var table = _allTables.GetById(tableId);
                if (table is null)
                    return;
                
                table.CurrentStage = RoundStageType.NotStarted;

                await HandlePlayersWithEmptyStackMoney(table);

                await WaitForPlayers(table);

                await RunNextStage(table);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
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
                    await RefreshTableState(table);
                    break;
                case RoundStageType.Refresh:
                    if (table.Type is TableType.SitAndGo && table.Players.Count is 1)
                        _allTables.Remove(table.Id);
                    if (table.Players.Count > 0)
                        await StartRound(table.Id);
                    break;
            }
        }

        // Preparation
        private async Task PrepareForGame(Table table)
        {
            try
            {
                table.CurrentStage = RoundStageType.PrepareTable;

                SetDealerAndBlinds(table);
                DealPocketCards(table);

                OnPrepareForGame?.Invoke(table);

                await Task.Delay(1000);

                await RunNextStage(table);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
        }

        // Game
        private async Task DealCommunityCards(Table table, int numberOfCards)
        {
            try
            {
                Log.Information("DealCommunityCards. Start");

                var cardsToAdd = _deckService.GetRandomCardsFromDeck(table.Deck, numberOfCards);
                table.CommunityCards.AddRange(cardsToAdd);

                OnDealCommunityCards?.Invoke(table, cardsToAdd);

                await Task.Delay(numberOfCards * 300);

                Log.Information("DealCommunityCards. End");

                if (table.ActivePlayers.Count > 1 && !table.IsEndDueToAllIn)
                    await StartWagering(table);
                else
                    await Showdown(table);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
        }

        private async Task StartWagering(Table table)
        {
            Log.Information("StartWagering. Start");

                ChooseCurrentStage(table);
                if (table.CurrentStage is RoundStageType.WageringPreFlopRound)
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
                    var currentPlayerId = table.CurrentPlayer.Id;

                    ReceiveCurrentPlayerIdInWagering?.Invoke(table, JsonSerializer.Serialize(currentPlayerId));

                    Log.Information(
                        $"StartWagering. Table after setting current player: {JsonSerializer.Serialize(table)}");
                    if (table.CurrentPlayer.GetType() == typeof(Bot))
                    {
                        var action = _botService.Act((Bot)table.CurrentPlayer, table);

                        Log.Information($"StartWagering. bot action: {JsonSerializer.Serialize(action)}");
                        table.CurrentPlayer.CurrentAction = action;

                        await Task.Delay(new Random().Next(1, 5) * 1000);
                    }
                    else
                    {
                        //TODO: Handle player's leaving during WaitOne
                        //TODO: Make timer
                        Log.Information($"StartWagering. Waiting");
                        table.WaitForPlayerBet.WaitOne();
                    }

                    if (table.ActivePlayers.Any(p => p.Id == currentPlayerId))
                    {
                        Log.Information($"StartWagering. Inside IF");
                        await ProcessPlayerAction(table, table.CurrentPlayer);

                        ReceivePlayerAction?.Invoke(table, table.CurrentPlayer.CurrentAction);
                    }

                    Log.Information($"StartWagering. Outside IF");

                    // Check if one of two players folded
                    if (table.ActivePlayers.Count is 1)
                    {
                        await Showdown(table);
                        return;
                    }

                    ReceiveTableState?.Invoke(table);

                    actionCounter--;
                } while (!CheckForWageringEnd(table, actionCounter));

                await Task.Delay(1000);

                foreach (var player in table.ActivePlayers)
                    player.CurrentBet = 0;

                // Check for end due to all in
                if (table.ActivePlayers.Count(p => p.StackMoney is not 0) <= 1)
                {
                    ReceiveUpdatedPot?.Invoke(table, JsonSerializer.Serialize(table.Pot));

                    await Task.Delay(table.ActivePlayers.Count * 300);

                    table.IsEndDueToAllIn = true;
                    await DealCommunityCards(table, 5 - table.CommunityCards.Count);
                    return;
                }

                // If at least one action was taken
                if (table.CurrentMaxBet > 0)
                {
                    ReceiveUpdatedPot?.Invoke(table, JsonSerializer.Serialize(table.Pot));

                    await Task.Delay(table.ActivePlayers.Count * 300);

                    table.CurrentMaxBet = 0;
                }

                table.CurrentPlayer = null;

                if (table.ActivePlayers.Count is 1)
                {
                    await Showdown(table);
                    return;
                }

                await Task.Delay(2500);
                await RunNextStage(table);
        }

        // End
        private async Task Showdown(Table table)
        {
            try
            {
                Log.Information("Showdown. Start");
                table.CurrentStage = RoundStageType.Showdown;

                table.SidePots = _cardEvaluationService.CalculateSidePotsWinners(table);

                GiveMoneyToWinners(table.SidePots);

                ReceiveWinners?.Invoke(table, JsonSerializer.Serialize(_mapper.Map<List<SidePotDto>>(table.SidePots)));

                ReceiveTableState?.Invoke(table);

                await Task.Delay(5000 + (table.ActivePlayers.Count * 500));

                Log.Information("Showdown. End");
                await RunNextStage(table);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
        }

        // CleanUp
        private async Task RefreshTableState(Table table)
        {
            try
            {
                table.CurrentStage = RoundStageType.Refresh;

                await UpdatePlayersStatistics(table);

                if (table.Type is TableType.SitAndGo)
                    await RefreshSitAndGoTable(table);

                RefreshTable(table);

                foreach (var player in table.Players.Where(p => p.PocketCards is not null))
                    player.NumberOfGamesOnCurrentTable++;

                RefreshPlayers(table);

                await Task.Delay(4000);

                OnGameEnd?.Invoke(table);

                await RunNextStage(table);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
        }

        #region Helpers

        // StartRound helpers
        private async Task HandlePlayersWithEmptyStackMoney(Table table)
        {
            Log.Information("AutoTop. Start");
            var minBuyIn = TableOptions.Tables[table.Title.ToString()]["MinBuyIn"];

            var playersForAutoTop = table
                .Players
                .Where(p => p.GetType() == typeof(Player))
                .Where(player => player.StackMoney is 0 && player.IsAutoTop && player.TotalMoney >= minBuyIn);

            foreach (var player in playersForAutoTop)
            {
                Log.Information($"AutoTop. Player: {JsonSerializer.Serialize(player)}");

                var currentTotalMoney = await _playerService.GetTotalMoney(player.Id);

                if (currentTotalMoney >= minBuyIn)
                {
                    await _playerService.AddStackMoneyFromTotalMoney(table.Id, player.Id, minBuyIn);

                    ReceivePlayerDto?.Invoke(player);
                }
            }

            foreach (var bot in table.Players.Where(p => p.GetType() == typeof(Bot)))
            {
                if (bot.StackMoney is 0)
                    bot.StackMoney = bot.CurrentBuyIn;
            }

            Log.Information("AutoTop. End");

            foreach (var player in table.Players.Where(p => p.StackMoney is 0))
            {
                OnLackOfStackMoney?.Invoke(player);
            }

            Log.Information("AutoTop. OnLackOfStackMoney is sent");
        }

        private async Task WaitForPlayers(Table table)
        {
            Log.Information("WaitForPlayers. Start");
            Log.Information($"WaitForPlayers. MinPlayersToStart: {table.MinPlayersToStart}");

            if (isGameWithBots)
                ManageBots(table);

            Log.Information($"WaitForPlayers. Table after managing bots: {JsonSerializer.Serialize(table)}");

            while (table.Players.Count(p => p.StackMoney > 0) < table.MinPlayersToStart)
                await Task.Delay(1000);

            Log.Information("WaitForPlayers. Number of players is enough to start");

            foreach (var player in table.Players.Where(p => p.StackMoney > 0))
            {
                table.ActivePlayers.Add(player);
                table.Pot.Bets.Add(player.Id, 0);
            }

            Log.Information("WaitForPlayers. Players are added to Active Players");

            while (table.ActivePlayers.Any(p => p.IsReady is not true))
                await Task.Delay(500);
            Log.Information("WaitForPlayers. End");
        }

        private void ManageBots(Table table)
        {
            try
            {
                if (table.Type is TableType.SitAndGo)
                    return;

                var maxNumberOfBots = table.MaxPlayers switch {8 => 3, _ => 2};
                var currentNumberOfBots = table.Players.Count(p => p.GetType() == typeof(Bot));

                // Remove bot if there are too much players
                if (table.Players.Count == table.MaxPlayers &&
                    currentNumberOfBots > 0)
                {
                    var botToRemove = table.Players.First(p => p.GetType() == typeof(Bot));

                    if (table.ActivePlayers.Contains(botToRemove))
                        table.ActivePlayers.Remove(botToRemove);

                    table.Players.Remove(botToRemove);

                    ReceiveTableState?.Invoke(table);
                }

                if (currentNumberOfBots < maxNumberOfBots &&
                    table.Players.Count + 2 <= table.MaxPlayers)
                {
                    var numberOfBotsToAdd =
                        table.MaxPlayers - 1 - table.Players.Count(p => p.GetType() == typeof(Player));

                    if (numberOfBotsToAdd > maxNumberOfBots)
                        numberOfBotsToAdd = maxNumberOfBots;

                    while (table.Players.Count(p => p.GetType() == typeof(Bot)) < numberOfBotsToAdd)
                    {
                        var bot = _botService.Create(table, BotComplexity.Hard);

                        table.Players.Add(bot);

                        ReceiveTableState?.Invoke(table);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}");
                Log.Error($"{e.StackTrace}");
                throw;
            }
        }

        // PrepareForGame helpers
        private void DealPocketCards(Table table)
        {
            Log.Information("DealPocketCards. Start");

            foreach (var player in table.ActivePlayers)
                player.PocketCards = _deckService.GetRandomCardsFromDeck(table.Deck, 2);

            Log.Information("DealPocketCards. End");
        }

        private void SetDealerAndBlinds(Table table)
        {
            Log.Information("SetDealerAndBlinds. Start");
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
                            table.DealerIndex = table.ActivePlayers
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

            Log.Information("SetDealerAndBlinds. End");
        }

        // StartWagering helpers
        private static void ChooseCurrentStage(Table table)
        {
            Log.Information("ChooseCurrentStage");
            table.CurrentStage = table.CurrentStage switch
            {
                RoundStageType.PrepareTable => RoundStageType.WageringPreFlopRound,
                RoundStageType.WageringPreFlopRound => RoundStageType.WageringSecondRound,
                RoundStageType.WageringSecondRound => RoundStageType.WageringThirdRound,
                RoundStageType.WageringThirdRound => RoundStageType.WageringFourthRound,
                _ => table.CurrentStage
            };
        }

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
                .All(player => player.CurrentAction.ActionType is PlayerActionType.Fold ||
                               player.CurrentBet == table.CurrentMaxBet);
            return result;
        }

        private void SetCurrentPlayer(Table table)
        {
            // if round starts and there is no current player
            if (table.CurrentPlayer is null)
            {
                table.CurrentPlayer = table.CurrentStage is RoundStageType.WageringPreFlopRound
                    ? GetNextPlayer(table, table.BigBlindIndex)
                    : GetNextPlayer(table, table.DealerIndex);
            }
            // if round has already started and there is a current player
            else
            {
                table.ActivePlayers = table.ActivePlayers.OrderBy(p => p.IndexNumber).ToList();
                table.CurrentPlayer = GetNextPlayer(table, table.CurrentPlayer.IndexNumber);
            }

            Player GetNextPlayer(Table table, int currentPlayerIndex)
            {
                Log.Information("Players before next player is chosen:");

                foreach (var player in table.ActivePlayers)
                {
                    Log.Information(
                        $"{player.UserName}, index: {player.IndexNumber}, stack: {player.StackMoney}");
                }
                
                var activePlayer = table.ActivePlayers
                    .FirstOrDefault(p => p.IndexNumber > currentPlayerIndex && p.StackMoney > 0);
                
                if (activePlayer is null)
                    activePlayer = table.ActivePlayers.First(p => p.StackMoney > 0);

                Log.Information($"New active player: {activePlayer.UserName}");
                return activePlayer;
            }
                
                
        }

        private async Task ProcessPlayerAction(Table table, Player player)
        {
            switch (player.CurrentAction.ActionType)
            {
                case PlayerActionType.Fold:
                    if (table.Type is TableType.DashPoker)
                    {
                        await _tableService.RemovePlayerFromTable(table.Id, player.Id);
                        RemoveFromGroupAsync?.Invoke(player.ConnectionId, table.Id.ToString());

                        PlayerDisconnected?.Invoke(table.Id.ToString(),
                            JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
                    }
                    else
                    {
                        table.ActivePlayers.Remove(player);
                        Log.Information("Player was removed from activePlayers");
                    }

                    break;

                // Check - If no one has yet opened the betting round (equivalent to betting zero)
                case PlayerActionType.Check:
                    break;

                // Bet - The opening bet of a betting round
                case PlayerActionType.Bet:
                    if (player.CurrentAction.Amount != null)
                    {
                        player.StackMoney -= (int) player.CurrentAction.Amount;
                        player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;

                        table.Pot.TotalAmount += (int) player.CurrentAction.Amount;
                        table.Pot.Bets[player.Id] += (int) player.CurrentAction.Amount;
                    }

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
                    if (player.CurrentAction.Amount != null)
                    {
                        player.StackMoney -= (int) player.CurrentAction.Amount;
                        player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount + player.CurrentBet;

                        table.Pot.TotalAmount += (int) player.CurrentAction.Amount;
                        table.Pot.Bets[player.Id] += (int) player.CurrentAction.Amount;
                    }

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

        private async Task MakeBlindBets(Table table)
        {
            await MakeSmallBlindBet(table);
            await MakeBigBlindBet(table);

            if (table.Type is not TableType.SitAndGo)
                await MakeNewPlayersBets(table);

            ReceiveTableState?.Invoke(table);
        }

        private async Task MakeBigBlindBet(Table table)
        {
            Log.Information("MakeBigBlindBet. Start");
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.BigBlindIndex);

            var bigBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Raise,
                Amount = table.BigBlind
            };

            if (player.StackMoney < table.BigBlind)
            {
                bigBlindAction.ActionType = PlayerActionType.AllIn;
                bigBlindAction.Amount = player.StackMoney;
            }

            player.CurrentAction = bigBlindAction;

            BigBlindBetEvent?.Invoke(table, JsonSerializer.Serialize(bigBlindAction));

            await ProcessPlayerAction(table, player);
            Log.Information("MakeBigBlindBet. End");
        }

        private async Task MakeSmallBlindBet(Table table)
        {
            Log.Information("MakeSmallBlindBet. Start");
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

            SmallBlindBetEvent?.Invoke(table, JsonSerializer.Serialize(smallBlindAction));

            await ProcessPlayerAction(table, player);
            Log.Information("MakeSmallBlindBet. End");
        }

        private async Task MakeNewPlayersBets(Table table)
        {
            Log.Information("MakeNewPlayersBets. Start");

            foreach (var player in table.ActivePlayers)
            {
                if (player.NumberOfGamesOnCurrentTable is 0 &&
                    table.BigBlindIndex != player.IndexNumber &&
                    table.SmallBlindIndex != player.IndexNumber)
                {
                    var bigBlindAction = new PlayerAction
                    {
                        PlayerIndexNumber = player.IndexNumber,
                        ActionType = PlayerActionType.Raise,
                        Amount = table.BigBlind
                    };

                    player.CurrentAction = bigBlindAction;

                    NewPlayerBetEvent?.Invoke(table, JsonSerializer.Serialize(bigBlindAction));

                    await ProcessPlayerAction(table, player);
                }
            }

            Log.Information("MakeNewPlayersBets. End");
        }

        // Showdown helpers 
        private static void GiveMoneyToWinners(List<SidePot> finalSidePots)
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
        private async Task UpdatePlayersStatistics(Table table)
        {
            foreach (var player in table.ActivePlayers.Where(p => p.GetType() == typeof(Player)))
            {
                var isWinner = table.SidePots
                    .First(sp => sp.Type is SidePotType.Main)
                    .Winners
                    .Contains(player);

                await _playerService.IncreaseNumberOfPlayedGamesAsync(player.Id, isWinner);

                if (isWinner)
                {
                    var winningSidePots = table
                        .SidePots
                        .Where(sp => sp.Winners.Contains(player))
                        .ToList();

                    var winAmount = winningSidePots
                        .Sum(sidePot => sidePot.WinningAmountPerPlayer);

                    var spentAmount = table.Pot.Bets[player.Id];

                    winAmount -= spentAmount;
                    
                    if (player.BiggestWin < winAmount)
                        await _playerService.ChangeBiggestWinAsync(player.Id, winAmount);
                }

                if (player.BestHandType < player.Hand)
                    await _playerService.ChangeBestHandTypeAsync(player.Id, (int) player.Hand);

                var experience = TableOptions.Tables[table.Title.ToString()]["Experience"];
                await _playerService.AddExperienceAsync(player.Id, experience);

                var moneySpent = table.Pot.Bets[player.Id];

                var win = table.SidePots
                    .Where(sidePot => sidePot.Winners.Contains(player))
                    .Sum(sidePot => sidePot.WinningAmountPerPlayer);

                await AddMoneyToMoneyBox(table, player, isWinner);
            }
        }

        private async Task AddMoneyToMoneyBox(Table table, Player player, bool isWinner)
        {
            if (isWinner)
            {
                var winningSidePots = table
                    .SidePots
                    .Where(sp => sp.Winners.Contains(player))
                    .ToList();
                
                var winAmount = winningSidePots
                    .Sum(sidePot => sidePot.WinningAmountPerPlayer);

                var spentAmount = table.Pot.Bets[player.Id];
                winAmount -= spentAmount;
                
                await _moneyBoxService.IncreaseMoneyBoxAmount(player.Id, winAmount / 20);
                
                return;
            }

            var lostAmount = table.Pot.Bets[player.Id];

            await _moneyBoxService.IncreaseMoneyBoxAmount(player.Id, Convert.ToInt32(lostAmount * 0.015));
        }

        private async Task RefreshSitAndGoTable(Table table)
        {
            Log.Information("RefreshSitAndGoTable. Start");

            if (table.SitAndGoRoundCounter is 4)
            {
                table.SmallBlind *= 2;
                table.BigBlind *= 2;

                table.SitAndGoRoundCounter = -1;
            }

            table.SitAndGoRoundCounter++;

            await RemoveSitAndGoPlayersWithEmptyStackMoney(table);

            // Handle winner
            if (table.Players.Count is 1)
            {
                Log.Information("RefreshSitAndGoTable. Start handling winner");

                var firstPlacePrize = TableOptions.Tables[table.Title.ToString()]["FirstPlacePrize"];
                Log.Information($"RefreshSitAndGoTable. firstPlacePrize: {firstPlacePrize}");

                await _playerService.AddTotalMoney(table.ActivePlayers[0].Id, firstPlacePrize);
                Log.Information("RefreshSitAndGoTable. Prize added to db");

                await _playerService.IncreaseNumberOfSitNGoWinsAsync(table.ActivePlayers[0].Id);

                EndSitAndGoGameFirstPlace?.Invoke(table);

                Log.Information("RefreshSitAndGoTable. EndSitAndGoGame is sent");
            }

            ReceiveTableState?.Invoke(table);

            table.MinPlayersToStart = table.Players.Count;
            Log.Information("RefreshSitAndGoTable. End");
        }

        private async Task RemoveSitAndGoPlayersWithEmptyStackMoney(Table table)
        {
            Log.Information(
                $"RemovePlayersWithEmptyStackMoney: players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");

            while (table.Players.Any(p => p.StackMoney is 0))
            {
                var playerToRemove = table
                    .Players
                    .First(p => p.StackMoney is 0);

                var playersPlace = table.Players.Count;

                Log.Information(
                    $"RemovePlayersWithEmptyStackMoney: players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");
                Log.Information(
                    $"RemovePlayersWithEmptyStackMoney: active players: {table.ActivePlayers.Count} : {JsonSerializer.Serialize(table.ActivePlayers)}");

                await _tableService.RemovePlayerFromTable(table.Id, playerToRemove.Id);

                Log.Information(
                    $"RemovePlayersWithEmptyStackMoney: new players: {table.Players.Count} : {JsonSerializer.Serialize(table.Players)}");
                Log.Information(
                    $"RemovePlayersWithEmptyStackMoney: new active players: {table.ActivePlayers.Count} : {JsonSerializer.Serialize(table.ActivePlayers)}");

                if (playersPlace is 2)
                {
                    var secondPlacePrize = TableOptions.Tables[table.Title.ToString()]["SecondPlacePrize"];
                    Log.Information($"RemovePlayersWithEmptyStackMoney: secondPlacePrize: {secondPlacePrize}");
                    await _playerService.AddTotalMoney(playerToRemove.Id, secondPlacePrize);
                }

                RemoveFromGroupAsync?.Invoke(playerToRemove.ConnectionId, table.Id.ToString());

                Log.Information($"RemovePlayersWithEmptyStackMoney: Place: {playersPlace}");
                EndSitAndGoGame?.Invoke(playerToRemove, playersPlace.ToString());
            }
        }

        private void RefreshTable(Table table)
        {
            table.Deck = _deckService.GetNewDeck(table.Type);
            table.Pot = new Pot();

            table.CommunityCards = new List<Card>(5);
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            table.IsEndDueToAllIn = false;
            table.SidePots = new List<SidePot>(table.MaxPlayers);
            table.ActivePlayers = new List<Player>(table.MaxPlayers);
        }

        private static void RefreshPlayers(Table table)
        {
            foreach (var player in table.Players)
            {
                player.PocketCards = new List<Card>(2);
                player.CurrentAction = null;
                player.CurrentBet = 0;
                player.Hand = HandType.None;
                player.HandValue = 0;
                player.HandCombinationCards = null;
            }
        }

        #endregion
    }
}