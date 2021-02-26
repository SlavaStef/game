using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Helpers.CardEvaluator.Hands;
using PokerHand.BusinessLogic.Helpers.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluator
{
    public static class CardEvaluator
    {
        public static List<Player> EvaluatePlayersHands(List<Card> communityCards, List<Player> players)
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