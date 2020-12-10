using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly List<Table> _allTables;
        private readonly IHubContext<GameHub> _hub;
        private readonly IMapper _mapper;
        private readonly ILogger<GameHub> _logger;

        public GameProcessManager(
            TablesCollection tablesCollection,
            IHubContext<GameHub> hubContext,
            IMapper mapper,
            ILogger<GameHub> logger)
        {
            _allTables = tablesCollection.Tables;
            _hub = hubContext;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task StartRound(Guid tableId)
        {
            _logger.LogInformation("StartRound. Start");
            var table = _allTables.First(t => t.Id == tableId);

            if (table.Players.Count < 2)
            {
                _logger.LogInformation($"StartRound. Table {table.Id}. Waiting for second player");
                await _hub.Clients.Group(table.Id.ToString()).SendAsync("WaitForPlayers");
            }

            while (table.Players.Count < 2)
                Thread.Sleep(1000);

            _logger.LogInformation("StartRound. Round started");

            table.ActivePlayers = table.Players.ToList();

            await SetDealerAndBlinds(table, _hub, _mapper, _logger);
            await DealPocketCards(table, _hub, _mapper, _logger);
            Thread.Sleep(1000); // wait for cards to be dealed
            await StartPreFlopWagering(table, _hub, _logger);
            await DealCommunityCards(table, 3, _hub, _logger);
            await StartWagering(table, _hub, _logger); // Deal 2-nd wagering
            await DealCommunityCards(table, 1, _hub, _logger); // Deal Turn card
            await StartWagering(table, _hub, _logger); // Deal 3-d wagering
            await DealCommunityCards(table, 1, _hub, _logger); // Deal River card
            await StartWagering(table, _hub, _logger); // Deal 4-th wagering
            await Showdown(table, _hub, _logger);
            
            _logger.LogInformation("StartRound. End");

            // start new round is there are any players
            if (table.Players.Count > 0)
            {
                _logger.LogInformation("StartRound. New round starts");
                RefreshTable(table);
                await StartRound(table.Id);
                Thread thread = new Thread(() => StartRound(tableId));
                thread.Start();
            }
        }

        private static async Task SetDealerAndBlinds(Table table, IHubContext<GameHub> hub, IMapper mapper, ILogger<GameHub> logger)
        {
            logger.LogInformation("SetDealerAndBlinds. Start");

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
            
            await hub.Clients.Group(table.Id.ToString()).SendAsync("SetDealerAndBlinds", JsonSerializer.Serialize(mapper.Map<TableDto>(table)));
            logger.LogInformation("SetDealerAndBlinds. Dealer and Blinds are set. New table is sent to all players");
            
            logger.LogInformation("SetDealerAndBlinds. End");
        }

        private static async Task DealPocketCards(Table table, IHubContext<GameHub> hub, IMapper mapper, ILogger<GameHub> logger)
        {
            logger.LogInformation("DealPocketCards. Start");

            table.CurrentStage = RoundStageType.DealPocketCards;
            
            foreach (var player in table.ActivePlayers)
                player.PocketCards = table.Deck.GetRandomCardsFromDeck(2);
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("DealPocketCards", JsonSerializer.Serialize(mapper.Map<List<PlayerDto>>(table.ActivePlayers)));
            logger.LogInformation("DealPocketCards. Pocket cards ara sent to players");
            logger.LogInformation("DealPocketCards. End");
        }

        private static async Task StartPreFlopWagering(Table table, IHubContext<GameHub> hub, ILogger<GameHub> logger)
        {
            logger.LogInformation("StartPreFlopWagering. Start");

            table.CurrentStage = RoundStageType.PreFlopWagering;

            logger.LogInformation(JsonSerializer.Serialize(table));
            
            await MakeSmallBlindBet(table, hub, logger);
            await MakeBigBlindBet(table, hub, logger);
            
            logger.LogInformation(JsonSerializer.Serialize(table));
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
            logger.LogInformation("StartPreFlopWagering. Table with blind bets is sent to players");
            logger.LogInformation(JsonSerializer.Serialize(table));
            
            var counter = table.ActivePlayers.Count;
            
            do
            {
                logger.LogInformation(JsonSerializer.Serialize(table));
                SetCurrentPlayer(table, logger);
                logger.LogInformation(JsonSerializer.Serialize(table));
                await hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveCurrentPlayerIdInWagering", JsonSerializer.Serialize(table.CurrentPlayer.Id));
                logger.LogInformation("StartPreFlopWagering. Current player is set and sent to all players");
                logger.LogInformation(JsonSerializer.Serialize(table));
                logger.LogInformation($"StartPreFlopWagering. Waiting for player {table.CurrentPlayer.Id} action");
                Waiter.WaitForPlayerBet.WaitOne();
                logger.LogInformation("StartPreFlopWagering. Player's action received. Next - processing");
                logger.LogInformation(JsonSerializer.Serialize(table));
                ProcessPlayerAction(table, table.CurrentPlayer, logger);
                logger.LogInformation("StartPreFlopWagering. Player's action processed. Ready to sent table to players");
                
                await hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
                
                counter--;
                logger.LogInformation("StartPreFlopWagering. New Table is sent to all players. End of cycle");
            }
            while (counter > 0 || !CheckIfAllBetsAreEqual(table.ActivePlayers, logger));
            logger.LogInformation("StartPreFlopWagering. All Players made their choices");
            logger.LogInformation(JsonSerializer.Serialize(table));
            
            CollectBetsFromTable(table, logger);
            logger.LogInformation($"StartPreFlopWagering. Table pot after collecting: {table.Pot}");
            
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            foreach (var player in table.ActivePlayers)
                player.CurrentBet = 0;
            
            logger.LogInformation(JsonSerializer.Serialize(table));
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceiveTableStateAtWageringEnd", JsonSerializer.Serialize(table));
            
            Thread.Sleep(2500);
            
            logger.LogInformation("StartPreFlopWagering. End");
        }
        
        private static async Task DealCommunityCards(Table table, int numberOfCards, IHubContext<GameHub> hub, ILogger<GameHub> logger)
        {
            logger.LogInformation("DealCommunityCards. Start");

            logger.LogInformation($"DealCommunityCards. Before adding stage: {JsonSerializer.Serialize(table)}");
            table.CurrentStage = RoundStageType.DealCommunityCards;
            logger.LogInformation("DealCommunityCards. Stage is set");
            
            var cardsToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            
            logger.LogInformation($"DealCommunityCards. CardsToAdd: {JsonSerializer.Serialize(cardsToAdd)}");
            
            logger.LogInformation($"DealCommunityCards. Before adding cards: {JsonSerializer.Serialize(table)}");
            table.CommunityCards = cardsToAdd.ToList();
            logger.LogInformation($"DealCommunityCards. After adding stage: {JsonSerializer.Serialize(table)}");
            logger.LogInformation("DealCommunityCards. CommunityCards added to table");
            
            await hub.Clients.Group(table.Id.ToString()).SendAsync("DealCommunityCards", JsonSerializer.Serialize(cardsToAdd));
            logger.LogInformation($"DealCommunityCards. Community cards ({numberOfCards}) are sent to all users");
            logger.LogInformation("DealCommunityCards. End");
        }

        private static async Task StartWagering(Table table, IHubContext<GameHub> hub, ILogger<GameHub> logger)
        {
            logger.LogInformation("StartWagering. Start");

            table.CurrentStage = RoundStageType.Wagering;
            
            var counter = table.ActivePlayers.Count;
            logger.LogInformation($"StartWagering. Counter: {counter}");

            do
            {
                // choose next player to make choice
                SetCurrentPlayer(table, logger);

                // player makes choice
                await hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveCurrentPlayerIdInWagering", JsonSerializer.Serialize(table.CurrentPlayer.Id));
                logger.LogInformation("StartWagering. Current player is set and sent to all players");
                
                logger.LogInformation("StartWagering. Waiting for player's action");
                Waiter.WaitForPlayerBet.WaitOne();
                logger.LogInformation("StartWagering. Player's action received");
                
                ProcessPlayerAction(table, table.CurrentPlayer, logger);
                logger.LogInformation("StartWagering. Player's action processed. Ready to sent table to players");
                
                await hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));

                counter--;
                logger.LogInformation($"StartWagering. New Table is sent to all players. End of cycle, counter: {counter}");
            } 
            while (counter > 0 || !CheckIfAllBetsAreEqual(table.ActivePlayers, logger));
            
            logger.LogInformation("StartWagering. All players made their choices");
            
            CollectBetsFromTable(table, logger);
            logger.LogInformation($"StartWagering. Table pot after collecting: {table.Pot}");
            
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            foreach (var player in table.ActivePlayers)
                player.CurrentBet = 0;
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceiveTableStateAtWageringEnd", JsonSerializer.Serialize(table));
            logger.LogInformation("StartWagering. Final table is sent to clients");
            
            Thread.Sleep(2500);
            
            logger.LogInformation("StartWagering. End");
        }

        private static async Task Showdown(Table table, IHubContext<GameHub> hub, ILogger<GameHub> logger)
        {
            logger.LogInformation("Showdown. Start");

            table.CurrentStage = RoundStageType.Showdown;
            
            var winners = CardsAnalyzer.DefineWinner(table.CommunityCards, table.ActivePlayers, logger);
            logger.LogInformation("Showdown. Winner(s) defined");

                table.Winners = winners.ToList();
                logger.LogInformation("Showdown. Winner(s) added to table");


            if (winners.Count == 1)
                winners[0].StackMoney += table.Pot;
            else
            {
                var winningAmount = table.Pot / winners.Count;

                foreach (var player in winners)
                    player.StackMoney += winningAmount;
            }
            
            await hub.Clients.Group(table.Id.ToString()).SendAsync("ReceiveWinners", JsonSerializer.Serialize(table.Winners));
            logger.LogInformation("Showdown. Winners are sent to players");
            
            logger.LogInformation("Showdown. End");
        }

        #region PrivateHelpers
        
        private static bool CheckIfAllBetsAreEqual(IReadOnlyCollection<Player> players, ILogger<GameHub> logger)
        {
            logger.LogInformation("CheckIfAllBetsAreEqual. Start");
            logger.LogInformation(JsonSerializer.Serialize(players));
            var amountToCompare = players
                .First(player => player.CurrentAction.ActionType != PlayerActionType.Fold)
                .CurrentBet;
            
            logger.LogInformation($"CheckIfAllBetsAreEqual. Amount to compare: {amountToCompare}");

            var result = players.All(player => 
                player.CurrentAction.ActionType == PlayerActionType.Fold || 
                player.CurrentBet == amountToCompare);
            
            logger.LogInformation($"CheckIfAllBetsAreEqual. Result: {result}");
            logger.LogInformation("CheckIfAllBetsAreEqual. End");
            return result;
        }

        private static void SetCurrentPlayer(Table table, ILogger<GameHub> logger)
        {
            logger.LogInformation("SetCurrentPlayer. Start");

            if (table.CurrentPlayer == null) // if round starts and there is no current player
            {
                table.CurrentPlayer = table.CurrentStage == RoundStageType.PreFlopWagering
                    ? GetNextPlayer(table, table.BigBlindIndex)
                    : GetNextPlayer(table, table.DealerIndex);
            }
            else // if round has already started and there is a current player
            {
                table.ActivePlayers = table.ActivePlayers.OrderBy(p => p.IndexNumber).ToList();
                table.CurrentPlayer = GetNextPlayer(table, table.CurrentPlayer.IndexNumber);
            }
            
            logger.LogInformation("SetCurrentPlayer. End");
        }
        
        private static void CollectBetsFromTable(Table table, ILogger<GameHub> logger)
        {
            logger.LogInformation("CollectBetsFromTable. Start");
            logger.LogInformation($"CollectBetsFromTable. Start pot = {table.Pot}");
            logger.LogInformation(JsonSerializer.Serialize(table));
            foreach (var player in table.ActivePlayers.Where(player => player.CurrentBet != 0))
            {
                table.Pot += player.CurrentBet;
            }
            logger.LogInformation($"CollectBetsFromTable. Final pot = {table.Pot}");
            logger.LogInformation(JsonSerializer.Serialize(table));
            logger.LogInformation("CollectBetsFromTable. End");
        }

        private static void ProcessPlayerAction(Table table, Player player, ILogger<GameHub> logger)
        {
            logger.LogInformation("ProcessPlayerAction. Start");
            logger.LogInformation(JsonSerializer.Serialize(table));
            switch (player.CurrentAction.ActionType)
            {
                case PlayerActionType.Fold:
                    table.Pot += player.CurrentBet;
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
            logger.LogInformation("ProcessPlayerAction. End");
            logger.LogInformation(JsonSerializer.Serialize(table));
        }

        private static async Task MakeSmallBlindBet(Table table, IHubContext<GameHub> hub, ILogger<GameHub> logger)
        {
            logger.LogInformation("MakeSmallBlindBet. Start");
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.SmallBlindIndex);
            
            var smallBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Bet,
                Amount = table.SmallBlind
            };

            player.CurrentAction = smallBlindAction;
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceivePlayerAction", JsonSerializer.Serialize(smallBlindAction));
            logger.LogInformation("MakeSmallBlindBet. Action is sent to players");
            
            ProcessPlayerAction(table, player, logger);
            
            logger.LogInformation("MakeSmallBlindBet. End");
        }
        
        private static async Task MakeBigBlindBet(Table table, IHubContext<GameHub> hub, ILogger<GameHub> logger)
        {
            logger.LogInformation("MakeBigBlindBet. Start");
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.BigBlindIndex);
            
            var bigBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Raise,
                Amount = table.BigBlind
            };

            player.CurrentAction = bigBlindAction;
            
            await hub.Clients.Group(table.Id.ToString())
                .SendAsync("ReceivePlayerAction", JsonSerializer.Serialize(bigBlindAction));
            logger.LogInformation("MakeBigBlindBet. Action is sent to players");
            
            ProcessPlayerAction(table, player, logger);
            
            logger.LogInformation("MakeBigBlindBet. End");
        }

        private static Player GetNextPlayer(Table table, int currentPlayerIndex)
        {
            return table.ActivePlayers.FirstOrDefault(p => p.IndexNumber > currentPlayerIndex) ?? table.ActivePlayers[0];
        }

        private static void RefreshTable(Table table)
        {
            table.Deck = new Deck();

            table.CurrentStage = RoundStageType.None;
            table.ActivePlayers = null;
            table.CommunityCards = null;
            table.CurrentPlayer = null;
            table.CurrentMaxBet = 0;
            table.Pot = 0;
            table.DealerIndex = -1;
            table.SmallBlindIndex = -1;
            table.BigBlindIndex = -1;
            table.Winners = null;
        }
        #endregion
    }
}