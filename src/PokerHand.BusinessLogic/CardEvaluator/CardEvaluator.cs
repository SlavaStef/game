using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.CardEvaluator.Hands;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.CardEvaluator
{
    public static class CardEvaluator
    {
        public static List<Player> EvaluatePlayersHands(List<Card> communityCards, List<Player> players, ILogger logger)
        {
            logger.LogInformation("EvaluatePlayersHands. Start");
            foreach (var player in players)
            {
                logger.LogInformation($"EvaluatePlayersHands. player: {JsonSerializer.Serialize(player.UserName)}");
                var result = FindCombination(player.PocketCards, communityCards);

                logger.LogInformation("EvaluatePlayersHands. 2");
                player.Hand = result.HandType;
                logger.LogInformation($"EvaluatePlayersHands. player.Hand: {JsonSerializer.Serialize(player.Hand)}");
                player.HandValue = result.Value;
                logger.LogInformation($"EvaluatePlayersHands. player.HandValue: {JsonSerializer.Serialize(player.HandValue)}");
                player.HandCombinationCards = result.Cards;
                logger.LogInformation($"EvaluatePlayersHands. player.HandCombinationCards: {JsonSerializer.Serialize(player.HandValue)}");
            }

            logger.LogInformation("EvaluatePlayersHands. End");
            return players;
        }
        
        private static EvaluatedHand FindCombination(List<Card> playerHand, List<Card> tableCards)
        {
            var listRules = new List<IRules>
            {
                new FiveOfAKind(),
                // new RoyalFlush(),
                new StraightFlush(), // TODO: finalize it
                new FourOfAKind(),
                new FullHouse(),
                new Flush(),
                new Straight(),
                new ThreeOfAKind(),
                new TwoPairs(),
                new OnePair(),
                new HighCard()
            };

            var result = new EvaluatedHand();
            
            foreach (var rule in listRules)
            {
                //TODO: remove isJokerGame
                var evaluationResult = rule.Check(playerHand, tableCards);
                
                if (evaluationResult.IsWinningHand)
                {
                    // TODO: create mapping rule
                    result.Value = evaluationResult.EvaluatedHand.Value;
                    result.HandType = evaluationResult.EvaluatedHand.HandType;
                    result.Cards = evaluationResult.EvaluatedHand.Cards;
                    
                    break;
                }
            }

            return result;
        }
    }
}