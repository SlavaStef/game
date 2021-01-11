using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
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
        private readonly List<Table> _allTables;
        private readonly IHubContext<GameHub, IGameHubClient> _hub;
        private readonly ITableService _tableService;
        private readonly IMapper _mapper;
        private readonly ILogger<GameHub> _logger;

        public GameProcessManager(
            TablesCollection tablesCollection,
            IHubContext<GameHub, IGameHubClient> hubContext,
            IMapper mapper,
            ILogger<GameHub> logger, 
            ITableService tableService)
        {
            _allTables = tablesCollection.Tables;
            _hub = hubContext;
            _mapper = mapper;
            _logger = logger;
            _tableService = tableService;
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
            await StartWagering(table);
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
            
            await StartWagering(table);
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
        
            if (table.CurrentStage == RoundStageType.WageringPreFlopRound)
            {
                await MakeSmallBlindBet(table);
                await MakeBigBlindBet(table);
            
                await _hub.Clients.Group(table.Id.ToString()).ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            }
            
            var counter = table.ActivePlayers.Count;
            _logger.LogInformation($"StartWagering. Counter: {counter}");
        
            do
            {
                // choose next player to make choice
                SetCurrentPlayer(table);
        
                // player makes choice
                await _hub.Clients.Group(table.Id.ToString()).ReceiveCurrentPlayerIdInWagering(JsonSerializer.Serialize(table.CurrentPlayer.Id));
                _logger.LogInformation("StartWagering. Current player is set and sent to all players");
                
                _logger.LogInformation("StartWagering. Waiting for player's action");
                table.WaitForPlayerBet.WaitOne();
                _logger.LogInformation("StartWagering. Player's action received");
                
                ProcessPlayerAction(table, table.CurrentPlayer);
                _logger.LogInformation("StartWagering. Player's action processed. Ready to sent table to players");
                
                await _hub.Clients.Group(table.Id.ToString()).ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
        
                counter--;
                _logger.LogInformation($"StartWagering. New Table is sent to all players. End of cycle, counter: {counter}");
            } 
            while (table.ActivePlayers.Count > 1 && (counter > 0 || !CheckIfAllBetsAreEqual(table)));
            
            _logger.LogInformation("StartWagering. All players made their choices");
        
            if (table.ActivePlayers.Count == 1)
            {
                _logger.LogInformation("StartWagering. There is one player. EdRoundIfFold");
                await EndRoundIfFold(table);
            }
            else
            {
                CollectBetsFromTable(table);
                _logger.LogInformation($"StartWagering. Table pot after collecting: {table.Pot}");
            
                table.CurrentPlayer = null;
                table.CurrentMaxBet = 0;
                foreach (var player in table.ActivePlayers)
                    player.CurrentBet = 0;
            
                await _hub.Clients.Group(table.Id.ToString()).ReceiveTableStateAtWageringEnd(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
                _logger.LogInformation("StartWagering. Final table is sent to clients");
            
                Thread.Sleep(2500);
            
                _logger.LogInformation("StartWagering. End");
        
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
        }
        
        // private async Task StartWagering(Table table)
        // {
        //     // Choose current stage based on the last one
        //     switch (table.CurrentStage)
        //     {
        //         case RoundStageType.DealPocketCards:
        //             table.CurrentStage = RoundStageType.WageringPreFlopRound;
        //             break;
        //         case RoundStageType.WageringPreFlopRound:
        //             table.CurrentStage = RoundStageType.WageringSecondRound;
        //             break;
        //         case RoundStageType.WageringSecondRound:
        //             table.CurrentStage = RoundStageType.WageringThirdRound;
        //             break;
        //         case RoundStageType.WageringThirdRound:
        //             table.CurrentStage = RoundStageType.WageringFourthRound;
        //             break;
        //     }
        //
        //     if (table.CurrentStage == RoundStageType.WageringPreFlopRound)
        //     {
        //         await MakeSmallBlindBet(table, hub, logger, mapper, tableService);
        //         await MakeBigBlindBet(table, hub, logger, mapper, tableService);
        //     
        //         await hub.Clients.Group(table.Id.ToString())
        //             .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
        //     }
        //     
        //     var counter = table.ActivePlayers.Count;
        //
        //     do
        //     {
        //         // choose next player to make choice
        //         SetCurrentPlayer(table, logger);
        //
        //         // player makes choice
        //         await hub.Clients.Group(table.Id.ToString())
        //             .SendAsync("ReceiveCurrentPlayerIdInWagering", JsonSerializer.Serialize(table.CurrentPlayer.Id));
        //         
        //         table.WaitForPlayerBet.WaitOne();
        //         ProcessPlayerAction(table, table.CurrentPlayer, hub, logger, mapper, tableService);
        //         
        //         await hub.Clients.Group(table.Id.ToString())
        //             .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
        //
        //         counter--;
        //     } 
        //     while (table.ActivePlayers.Count > 1 && (counter > 0 || !CheckIfAllBetsAreEqual(table, logger)));
        //
        //     if (table.ActivePlayers.Count == 1)
        //     {
        //         await EndRoundIfFold(table, hub, mapper, logger);
        //     }
        //     else
        //     {
        //         CollectBetsFromTable(table, logger);
        //         
        //         table.CurrentPlayer = null;
        //         table.CurrentMaxBet = 0;
        //         foreach (var player in table.ActivePlayers)
        //             player.CurrentBet = 0;
        //     
        //         await hub.Clients.Group(table.Id.ToString())
        //             .SendAsync("ReceiveTableStateAtWageringEnd", JsonSerializer.Serialize(table));
        //         
        //         Thread.Sleep(2500);
        //     
        //         switch (table.CurrentStage)
        //         {
        //             case RoundStageType.WageringPreFlopRound:
        //                 await DealCommunityCards(table, 3, hub, mapper, logger, tableService);
        //                 break;
        //             case RoundStageType.WageringFourthRound:
        //                 await Showdown(table, hub, mapper, logger);
        //                 break;
        //             default:
        //                 await DealCommunityCards(table, 1, hub, mapper, logger, tableService);
        //                 break;
        //         }
        //     }
        // }

        private async Task Showdown(Table table)
        {
            _logger.LogInformation("Showdown. Start");

            table.CurrentStage = RoundStageType.Showdown;
            
            // logger.LogInformation("Showdown. Community cards:");
            // foreach (var card in table.CommunityCards)
            // {
            //     
            //         logger.LogInformation($"            Card: {card.Rank}, {card.Suit}");
            //     
            // }
            //
            // var winners = CardsAnalyzer.DefineWinner(table.CommunityCards, table.ActivePlayers, logger);
            // logger.LogInformation("Showdown. Winner(s) defined");
            //
            //     table.Winners = winners.ToList();
            //     logger.LogInformation("Showdown. Winner(s) added to table");
            //
            //
            // if (winners.Count == 1)
            //     winners[0].StackMoney += table.Pot;
            // else
            // {
            //     var winningAmount = table.Pot / winners.Count;
            //
            //     foreach (var player in winners)
            //         player.StackMoney += winningAmount;
            // }

            table.Winners = new List<Player>();
            var random = new Random();

            var randomNumber = random.Next(1, 3);

            switch (randomNumber)
            {
                case 1:
                    table.Winners.Add(table.ActivePlayers[0]);
                    break;
                case 2:
                    table.Winners.Add(table.ActivePlayers[1]);
                    break;
                default:
                    table.Winners.Add(table.ActivePlayers[0]);
                    table.Winners.Add(table.ActivePlayers[1]);
                    break;
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

        private void SetCurrentPlayer(Table table)
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
            
            _logger.LogInformation("SetCurrentPlayer. End");
        }
        
        private void CollectBetsFromTable(Table table)
        {
            _logger.LogInformation("CollectBetsFromTable. Start");
            _logger.LogInformation($"CollectBetsFromTable. Start pot = {table.Pot}");
            _logger.LogInformation(JsonSerializer.Serialize(table));
            foreach (var player in table.ActivePlayers.Where(player => player.CurrentBet != 0))
            {
                table.Pot += player.CurrentBet;
            }
            _logger.LogInformation($"CollectBetsFromTable. Final pot = {table.Pot}");
            _logger.LogInformation(JsonSerializer.Serialize(table));
            _logger.LogInformation("CollectBetsFromTable. End");
        }

        private async void ProcessPlayerAction(Table table, Player player)
        {
            _logger.LogInformation("ProcessPlayerAction. Start");
            _logger.LogInformation(JsonSerializer.Serialize(table));
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
            _logger.LogInformation(JsonSerializer.Serialize(table));
        }

        private async Task MakeSmallBlindBet(Table table)
        {
            _logger.LogInformation("MakeSmallBlindBet. Start");
            var player = table.ActivePlayers.First(p => p.IndexNumber == table.SmallBlindIndex);
            
            var smallBlindAction = new PlayerAction
            {
                PlayerIndexNumber = player.IndexNumber,
                ActionType = PlayerActionType.Bet,
                Amount = table.SmallBlind
            };

            player.CurrentAction = smallBlindAction;
            
            await _hub.Clients.Group(table.Id.ToString()).ReceivePlayerAction(JsonSerializer.Serialize(smallBlindAction));
            _logger.LogInformation("MakeSmallBlindBet. Action is sent to players");
            
            ProcessPlayerAction(table, player);
            
            _logger.LogInformation("MakeSmallBlindBet. End");
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

            player.CurrentAction = bigBlindAction;
            
            await _hub.Clients.Group(table.Id.ToString()).ReceivePlayerAction(JsonSerializer.Serialize(bigBlindAction));
            _logger.LogInformation("MakeBigBlindBet. Action is sent to players");
            
            ProcessPlayerAction(table, player);
            
            _logger.LogInformation("MakeBigBlindBet. End");
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