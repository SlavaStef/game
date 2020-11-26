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
            await DealCommunityCards(table, 3);
            await StartWagering(table); // Deal 2-nd wagering
            await DealCommunityCards(table, 1); // Deal Turn card
            await StartWagering(table); // Deal 3-d wagering
            await DealCommunityCards(table, 1); // Deal River card
            await StartWagering(table); // Deal 4-th wagering
            await Showdown(table);

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
            _logger.LogInformation($"Table pot after collecting: {table.Pot}");
            
            await _hub.Clients.All
                .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));

            table.CurrentPlayer = null;
            
            _logger.LogInformation($"table {table.Id}: Method StartPreFlopWagering ends");
        }
        
        private async Task DealCommunityCards(Table table, int numberOfCards)
        {
            var cardsToAdd = table.Deck.GetRandomCardsFromDeck(numberOfCards);
            table.CommunityCards.AddRange(cardsToAdd);
            
            await _hub.Clients.Group(table.Id.ToString()).SendAsync("DealCommunityCards", JsonSerializer.Serialize(table.CommunityCards));
            _logger.LogInformation($"Community cards ({numberOfCards}) are sent to all users");
        }

        private async Task StartWagering(Table table)
        {
            _logger.LogInformation($"table {table.Id}: Method StartWagering starts");

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
                _logger.LogInformation($"table {table.Id}: Method StartWagering. Player's action recieved");
                
                ProcessPlayerAction(table, table.CurrentPlayer);
                await _hub.Clients.Group(table.Id.ToString())
                    .SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));
            } 
            while (CheckIfAllBetsAreEqual(table.Players));

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
                var currentPlayerIndex = table.CurrentPlayer.IndexNumber;
                table.CurrentPlayer = table.Players.First(player => player.IndexNumber == currentPlayerIndex + 1);
            }
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

    public static class CardsAnalyzer
    {
        public static List<Player> DefineWinner(List<Card> communityCards, List<Player> players)
        {
            var maxHand = 0;
            
            foreach (var player in players)
            {
                var totalCards = new List<Card>();
                totalCards.AddRange(communityCards);
                totalCards.AddRange(player.PocketCards);

                player.Hand = AnalyzePlayerCards(totalCards);
                
                if ((int) player.Hand > maxHand)
                    maxHand = (int) player.Hand;
            }

            var winners = players.FindAll(player => (int) player.Hand == maxHand);

            return winners;
        }
        
        public static HandType AnalyzePlayerCards(List<Card> cards)
        {
            if (IsRoyalFlash(cards))
                return HandType.RoyalFlush;

            if (IsStraightFlush(cards))
                return HandType.StraightFlush;

            if (IsFourOfAKind(cards))
                return HandType.FourOfAKind;

            if (IsFullHouse(cards))
                return HandType.FullHouse;

            if (IsFlush(cards))
                return HandType.Flush;

            if (IsStraight(cards))
                return HandType.Straight;

            if (IsThreeOfAKind(cards))
                return HandType.ThreeOfAKind;

            if (IsTwoPairs(cards))
                return HandType.TwoPairs;

            if (IsOnePair(cards))
                return HandType.OnePair;

            return HandType.HighCard;
        }
        
        public static Card GetHighCard(List<Card> cards) =>
            SortByRank(cards)[4];
        
        
        private static bool IsRoyalFlash(List<Card> cards) =>
            IsStraight(cards) && IsFlush(cards) && (int) SortByRank(cards)[4].Rank == 12;
        
        private static bool IsStraightFlush(List<Card> cards) =>
                    IsStraight(cards) && IsFlush(cards);
        
        private static bool IsFourOfAKind(List<Card> cards)
        {
            cards = SortByRank(cards);

            var isFourWithHigherCard = (int) cards[0].Rank == (int) cards[1].Rank &&
                                       (int) cards[1].Rank == (int) cards[2].Rank &&
                                       (int) cards[2].Rank == (int) cards[3].Rank;
            
            var isFourWithLowerCard = (int) cards[1].Rank == (int) cards[2].Rank &&
                                      (int) cards[2].Rank == (int) cards[3].Rank &&
                                      (int) cards[3].Rank == (int) cards[4].Rank;

            return isFourWithHigherCard || isFourWithLowerCard;
        }
        
        private static bool IsFullHouse(List<Card> cards)
        {
            cards = SortByRank(cards);

            var isThreeLower = (int) cards[0].Rank == (int) cards[1].Rank &&
                               (int) cards[1].Rank == (int) cards[2].Rank &&
                               (int) cards[3].Rank == (int) cards[4].Rank;
            
            var isThreeHigher = (int) cards[0].Rank == (int) cards[1].Rank &&
                                (int) cards[2].Rank == (int) cards[3].Rank &&
                                (int) cards[3].Rank == (int) cards[4].Rank;

            return isThreeLower || isThreeHigher;
        }
        
        private static bool IsFlush(List<Card> cards) =>
            cards.All(card => card.Suit == cards[0].Suit);

        private static bool IsStraight(List<Card> cards)
        {
            cards = SortByRank(cards);

            if ((int) cards[4].Rank == 13)
            {
                var isFiveHighStraight = (int) cards[0].Rank == 1 && (int) cards[1].Rank == 2 &&
                         (int) cards[2].Rank == 3 && (int) cards[3].Rank == 4;
                var isAceHighStraight = (int) cards[0].Rank == 9 && (int) cards[1].Rank == 10 && 
                         (int) cards[2].Rank == 11 && (int) cards[3].Rank == 12;

                return isFiveHighStraight || isAceHighStraight;
            }
            else
            {
                var testRank = (int) cards[0].Rank + 1;

                for (var i = 0; i < 5; i++)
                {
                    if ((int) cards[i].Rank != testRank)
                        return false;

                    testRank++;
                }

                return true;
            }
        }

        private static bool IsThreeOfAKind(List<Card> cards)
        {
            cards = SortByRank(cards);

            var isThreeAtBeginning = (int) cards[1].Rank == (int) cards[2].Rank && 
                                     (int) cards[2].Rank == (int) cards[3].Rank && 
                                     (int) cards[0].Rank != (int) cards[1].Rank && 
                                     (int) cards[4].Rank != (int) cards[1].Rank && 
                                     (int) cards[0].Rank != (int) cards[4].Rank;
            
            var isThreeInMiddle = (int) cards[1].Rank == (int) cards[2].Rank && 
                                     (int) cards[2].Rank == (int) cards[3].Rank && 
                                     (int) cards[0].Rank != (int) cards[1].Rank && 
                                     (int) cards[4].Rank != (int) cards[1].Rank && 
                                     (int) cards[0].Rank != (int) cards[4].Rank;
            
            var isThreeInEnd = (int) cards[0].Rank == (int) cards[1].Rank &&
                                  (int) cards[1].Rank == (int) cards[2].Rank &&
                                  (int) cards[3].Rank != (int) cards[0].Rank &&
                                  (int) cards[4].Rank != (int) cards[0].Rank &&
                                  (int) cards[3].Rank != (int) cards[4].Rank;

            return isThreeAtBeginning || isThreeInMiddle || isThreeInEnd;
        }
        
        private static bool IsTwoPairs(List<Card> cards)
        {
            cards = SortByRank(cards);

            var isTwoPairsAtBeginning = (int) cards[0].Rank == (int) cards[1].Rank &&
                                      (int) cards[1].Rank == (int) cards[3].Rank;
            var isThoPairsOnSides = (int) cards[0].Rank == (int) cards[1].Rank &&
                                      (int) cards[3].Rank == (int) cards[4].Rank;
            var isTwoPairsInEnd = (int) cards[1].Rank == (int) cards[2].Rank &&
                                      (int) cards[3].Rank == (int) cards[4].Rank;

            return isTwoPairsAtBeginning || isThoPairsOnSides || isTwoPairsInEnd;
        }

        private static bool IsOnePair(List<Card> cards)
        {
            cards = SortByRank(cards);

            var isFirstEqualsSecond = (int) cards[0].Rank == (int) cards[1].Rank;
            var isSecondEqualsThird = (int) cards[1].Rank == (int) cards[2].Rank;
            var isThirdEqualsFourth = (int) cards[2].Rank == (int) cards[3].Rank;
            var isFourthEqualsFifth = (int) cards[3].Rank == (int) cards[4].Rank;

            return isFirstEqualsSecond || isSecondEqualsThird || isThirdEqualsFourth || isFourthEqualsFifth;
        }

        private static List<Card> SortByRank(List<Card> cards)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var minimalCardIndex = i;

                for (var j = i + 1; j < cards.Count; j++)
                {
                    if ((int)cards[j].Rank < (int)cards[minimalCardIndex].Rank)
                        minimalCardIndex = j;
                }
                
                var tempCard = cards[i];
                cards[i] = cards[minimalCardIndex];
                cards[minimalCardIndex] = tempCard;
            }

            return cards;
        }
    }
}