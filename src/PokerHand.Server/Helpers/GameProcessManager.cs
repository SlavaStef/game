using System;
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
using Serilog;

namespace PokerHand.Server.Helpers
{
    public interface IGameProcessManager
    {
        Task StartRound(Guid tableId);
        Task StartSitAndGoRound(Guid tableId);
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
                await _hub.Clients.Group(table.Id.ToString())
                    .WaitForPlayers();
            }

            while (table.Players.Count(p => p.StackMoney >= table.BigBlind) < 2)
                Thread.Sleep(1000);

            _logger.LogInformation("StartRound. Round started");

            // Add players to Active Players
            foreach (var player in table.Players.Where(p => p.StackMoney >= table.BigBlind))
                table.ActivePlayers.Add(player);

            while (table.ActivePlayers.Any(p => p.IsReady != true))
                Thread.Sleep(500);
            _logger.LogInformation("StartRound. Start with two players");
            
            await PrepareForGame(table); // Start chain of game methods
            
            _logger.LogInformation("StartRound. End");

            // Start new round is there is any player
            if (table.Players.Count > 0)
            {
                _logger.LogInformation("StartRound. New round starts");
                await RefreshTable(table);
                await AutoTopStackMoney(table);
                await StartRound(table.Id);
            }
        }
        
        public async Task StartSitAndGoRound(Guid tableId)
        {
            _logger.LogInformation("StartRound. Start");
            var table = _allTables.First(t => t.Id == tableId);

            while (table.Players.Count < table.MaxPlayers)
                Thread.Sleep(1000);

            _logger.LogInformation("StartRound. Round started");

            table.ActivePlayers = table.Players.ToList();
            
            while (table.ActivePlayers.Any(p => p.IsReady != true))
                Thread.Sleep(500);
            
            await PrepareForGame(table); // Start chain of game methods
            
            _logger.LogInformation("StartRound. End");

            if (table.Players.Count > 1)
            {
                _logger.LogInformation("StartRound. New round starts");
                RefreshTable(table);
                await AutoTopStackMoney(table);
                await StartRound(table.Id);
            }
            else
            {
                _tableService.RemoveTable(table.Id);
            }
            
        }


        // Preparation
        private async Task PrepareForGame(Table table)
        {
            await SetDealerAndBlinds(table);
            await DealPocketCards(table);
            
            if (table.ActivePlayers.Count > 1)
                await StartWagering(table);
            else
                await EndRoundNow(table);
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

            if (table.ActivePlayers.Count > 1)
                await StartWagering(table);
            else
                await EndRoundNow(table);
        }
        
        private async Task StartWagering(Table table)
        {
            _logger.LogInformation("StartWagering. Start");
            
            ChooseCurrentStage(table);
            await MakeBlindBets(table);
            
            // Counter is used to make sure all players acted at least one time
            var actionCounter = table.ActivePlayers.Count;
            _logger.LogInformation($"StartWagering. Counter: {actionCounter}");
        
            // Players make choices
            do
            {
                if (table.ActivePlayers.Count < 1)
                {
                    await EndRoundNow(table);
                    return;
                }

                // choose next player to make choice
                await SetCurrentPlayer(table);
        
                _logger.LogInformation("StartWagering. Waiting for player's action");
                table.WaitForPlayerBet.WaitOne(); //TODO: Handle player's leaving during WaitOne
                _logger.LogInformation("StartWagering. Player's action received");
                
                await ProcessPlayerAction(table, table.CurrentPlayer);
                if (table.ActivePlayers.Count == 1)
                {
                    await EndRoundNow(table);
                    return;
                }
                
                await _hub.Clients.Group(table.Id.ToString())
                    .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
        
                actionCounter--;
                _logger.LogInformation($"StartWagering. New Table is sent to all players. End of cycle, counter: {actionCounter}");
            } 
            while (table.ActivePlayers.Count > 1 
                   && (actionCounter > 0 || !CheckIfAllBetsAreEqual(table)));
            
            _logger.LogInformation("StartWagering. All players made their choices");
        
            CollectBetsFromTable(table);
            
            if (table.CurrentMaxBet > 0)
                Thread.Sleep(500);

            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            foreach (var player in table.ActivePlayers)
                player.CurrentBet = 0;

            if (table.ActivePlayers.Count == 1)
            {
                _logger.LogInformation("StartWagering. There is one player. EdRoundIfFold");
                await EndRoundNow(table);
                return;
            }
            
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

        // End
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
            
            Thread.Sleep(5000 + (table.ActivePlayers.Count * 200)); 
            
            _logger.LogInformation("Showdown. End");

            // Remove players from SitAndGo
            if (table.Type == TableType.SitAndGo)
            {
                TableDto newTableDto = null;

                foreach (var player in table.ActivePlayers.Where(p => p.StackMoney == 0))
                {
                    newTableDto = await _tableService.RemovePlayerFromSitAndGoTable(table.Id, player.Id);
                    await _hub.Groups.RemoveFromGroupAsync(player.ConnectionId, table.Id.ToString());
                    await _hub.Clients.Client(player.ConnectionId).EndSitAndGoGame("1");
                }

                if (newTableDto != null)
                    await _hub.Clients.Group(table.Id.ToString())
                        .ReceiveTableState(JsonSerializer.Serialize(newTableDto));
            }
        }

        private async Task EndRoundNow(Table table)
        {
            _logger.LogInformation("EndRoundNow. Start");
            table.Winners.Add(table.ActivePlayers.First());
            
            await _hub.Clients.Group(table.Id.ToString())
                .ReceiveWinners(JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.Winners)));

            Thread.Sleep(6000 + (table.ActivePlayers.Count * 200));
            
            _logger.LogInformation("EndRoundNow. End");
        }

        #region PrivateHelpers
        
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
        }

        private async Task DealPocketCards(Table table)
        {
            _logger.LogInformation("DealPocketCards. Start");

            table.CurrentStage = RoundStageType.DealPocketCards;
            
            foreach (var player in table.ActivePlayers)
            {
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);
            }
            
            await _hub.Clients.Group(table.Id.ToString())
                .DealPocketCards(JsonSerializer.Serialize(_mapper.Map<List<PlayerDto>>(table.ActivePlayers)));
            _logger.LogInformation("DealPocketCards. End");
            
            Thread.Sleep(1000); // wait for the cards to be dealed
        }
        
        private bool CheckIfAllBetsAreEqual(Table table)
        {
            _logger.LogInformation("CheckIfAllBetsAreEqual. Start");

            var result = table.ActivePlayers.All(player => 
                player.CurrentAction.ActionType == PlayerActionType.Fold || 
                player.CurrentBet == table.CurrentMaxBet);
            
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
        
        private static void ChooseCurrentStage(Table table)
        {
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
                    player.StackMoney -= (int) player.CurrentAction.Amount;
                    player.CurrentBet = table.CurrentMaxBet = (int) player.CurrentAction.Amount;
                    _logger.LogInformation("BET. End");
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
            
            await ProcessPlayerAction(table, player);
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
            
            await ProcessPlayerAction(table, player);
        }

        private static Player GetNextPlayer(Table table, int currentPlayerIndex)
        {
            return table.ActivePlayers.FirstOrDefault(p => p.IndexNumber > currentPlayerIndex) ?? table.ActivePlayers[0];
        }

        private async Task RefreshTable(Table table)
        {
            table.Deck = new Deck(table.Type);

            table.CurrentStage = RoundStageType.None;
            table.CommunityCards = new List<Card>(5);
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            table.Pot = 0;
            table.DealerIndex = -1;
            table.SmallBlindIndex = -1;
            table.BigBlindIndex = -1;
            table.Winners = new List<Player>(table.MaxPlayers);

            if (table.Type == TableType.SitAndGo)
            {
                table.SmallBlind *= 2;
                table.BigBlind *= 2;
            }

            foreach (var player in table.ActivePlayers.Where(p => p.StackMoney < table.BigBlind))
            {
                await _hub.Clients.Client(player.ConnectionId)
                    .OnLackOfStackMoney();
            }
            
            table.ActivePlayers = new List<Player>(table.MaxPlayers);
            
            Thread.Sleep(5000);
        }
        
        private async Task AutoTopStackMoney(Table table)
        {
            foreach (var player in table.Players)
            {
                if (player.StackMoney == 0 && player.IsAutoTop && player.TotalMoney >= player.CurrentBuyIn)
                {
                    var isGotMoneyFromTotalMoney = await _playerService.GetStackMoney(player.Id, player.CurrentBuyIn);
                    
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