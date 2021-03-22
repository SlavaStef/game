using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Services
{
    public class CardEvaluationService : ICardEvaluationService
    {
        private readonly ILogger<CardEvaluationService> _logger;

        public CardEvaluationService(ILogger<CardEvaluationService> logger)
        {
            _logger = logger;
        }

        public Player EvaluatePlayerHand(List<Card> communityCards, Player player)
        {
            try
            {
                return EvaluatePlayersHands(communityCards, new List<Player> {player}).First();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        public List<SidePot> CalculateSidePotsWinners(Table table)
        {
            _logger.LogInformation($"CalculateSidePotsWinners. Table: {JsonSerializer.Serialize(table)}");

            if (table.ActivePlayers.Count is 1)
            {
                return new List<SidePot>
                {
                    new()
                    {
                        Type = SidePotType.Main,
                        WinningAmountPerPlayer = table.Pot.TotalAmount,
                        Winners = new List<Player>
                        {
                            table.ActivePlayers.First()
                        }
                    }
                };
            }

            // 1. Evaluate players' cards => get a list of players with HandValue
            var playersWithEvaluatedHands = EvaluatePlayersHands(table.CommunityCards, table.ActivePlayers);
            _logger.LogInformation(
                $"CalculateSidePotsWinners. Players: {JsonSerializer.Serialize(playersWithEvaluatedHands)}");

            // 2. Get a list of sidePots with Type, Players, AmountOfOneBet, TotalAmount
            var sidePots = CreateSidePots(table);
            _logger.LogInformation($"CalculateSidePotsWinners. SidePots: {JsonSerializer.Serialize(sidePots)}");

            // 3.Get final sidePots
            var finalSidePots = GetSidePotsWithWinners(sidePots, playersWithEvaluatedHands);
            _logger.LogInformation(
                $"CalculateSidePotsWinners. SidePots: {JsonSerializer.Serialize(finalSidePots)}");

            return finalSidePots;
        }

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
                if (sidePot.Players.Count is 1)
                {
                    sidePot.Winners = sidePot.Players.ToList();
                    sidePot.WinningAmountPerPlayer = sidePot.TotalAmount;
                    break;
                }

                DefineWinnersInSidePot(players, sidePot);

                if (sidePot.Winners.Count is not 0)
                    sidePot.WinningAmountPerPlayer = sidePot.TotalAmount / sidePot.Winners.Count;
                else
                    sidePot.WinningAmountPerPlayer = sidePot.TotalAmount;
            }

            _logger.LogInformation($"GetSidePotsWithWinners. SidePots: {JsonSerializer.Serialize(sidePots)}");
            return sidePots;
        }

        private static void DefineWinnersInSidePot(List<Player> players, SidePot sidePot)
        {
            for (var handTypeIndex = (int) HandType.FiveOfAKind; handTypeIndex > (int) HandType.None; handTypeIndex--)
            {
                var playersWithCurrentHand = players
                    .Where(p => (int) p.Hand == handTypeIndex)
                    .ToList();

                switch (playersWithCurrentHand.Count)
                {
                    case 0:
                        continue;
                    case 1:
                        sidePot.Winners = playersWithCurrentHand.ToList();
                        break;
                    default:
                        for (var cardIndex = 0; cardIndex < 5; cardIndex++)
                        {
                            SubstituteJokers(playersWithCurrentHand, cardIndex);

                            var currentMaxRank = playersWithCurrentHand
                                .Select(player => (int) player.HandCombinationCards[cardIndex].Rank)
                                .Prepend(0)
                                .Max();

                            playersWithCurrentHand = playersWithCurrentHand
                                .Where(p => (int) p.HandCombinationCards[cardIndex].Rank == currentMaxRank)
                                .ToList();

                            RestoreJokers(playersWithCurrentHand, cardIndex);

                            if (playersWithCurrentHand.Count is 1)
                            {
                                sidePot.Winners = playersWithCurrentHand.ToList();
                                break;
                            }
                        }

                        sidePot.Winners = playersWithCurrentHand.ToList();
                        break;
                }

                if (sidePot.Winners.Count > 0)
                    break;
            }
        }

        private static void RestoreJokers(List<Player> playersWithCurrentHand, int cardIndex)
        {
            foreach (var player in playersWithCurrentHand)
            {
                var currentCard = player.HandCombinationCards[cardIndex];

                if (currentCard.SubstitutedCard is not null)
                    currentCard.Rank = CardRankType.Joker;
            }
        }

        private static void SubstituteJokers(List<Player> playersWithCurrentHand, int cardIndex)
        {
            foreach (var player in playersWithCurrentHand)
            {
                var currentCard = player.HandCombinationCards[cardIndex];

                if (currentCard.Rank is CardRankType.Joker)
                    currentCard.Rank = currentCard.SubstitutedCard.Rank;
            }
        }

        private List<Player> EvaluatePlayersHands(List<Card> communityCards, List<Player> players)
        {
            try
            {
                foreach (var player in players)
                {
                    var result = FindCombination(player.PocketCards, communityCards);

                    player.Hand = result.HandType;
                    player.HandValue = result.Value;
                    player.HandCombinationCards = result.Cards;
                }

                return players;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }

        private Hand FindCombination(List<Card> playerHand, List<Card> tableCards)
        {
            try
            {
                var result = new Hand();

                var listRules = new List<IRules>
                {
                    new FiveOfAKind(), // +++
                    new RoyalFlush(), // + no joker logic
                    new StraightFlush(), // + no joker logic
                    new FourOfAKind(), // +++
                    new FullHouse(), // +++
                    new Flush(), // +++
                    new Straight(), // +++
                    new ThreeOfAKind(), // +++
                    new TwoPairs(), // ++
                    new OnePair(), // ++
                    new HighCard() // +
                };

                foreach (var rule in listRules)
                {
                    //TODO: remove isJokerGame
                    var evaluationResult = rule.Check(playerHand, tableCards);

                    if (evaluationResult.IsWinningHand is false)
                        continue;

                    // TODO: create mapping rule
                    result.Value = evaluationResult.Hand.Value;
                    result.HandType = evaluationResult.Hand.HandType;
                    result.Cards = evaluationResult.Hand.Cards;

                    break;
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
        }
    }
}