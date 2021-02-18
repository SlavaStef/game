using System.Collections.Generic;
using PokerHand.BusinessLogic.CardEvaluator.Hands;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.CardEvaluator
{
    public static class CardEvaluator
    {
        public static List<Player> EvaluatePlayersHands(List<Card> communityCards, List<Player> players, bool isJokerGame)
        {
            //var currentMaxValue = 0;
            
            foreach (var player in players)
            {
                var result = FindCombination(player.PocketCards, communityCards, isJokerGame);

                player.Hand = result.HandType;
                player.HandValue = result.Value;
                player.HandCombinationCards = result.Cards;

                // if (player.HandValue > currentMaxValue)
                //     currentMaxValue = player.HandValue;
            }

            // var winners = players.Where(p => p.HandValue == currentMaxValue).ToList();
            //
            // return winners;

            return players;
        }
        
        private static EvaluatedHand FindCombination(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var listRules = new List<IRules>
            {
                // new RoyalFlush(),
                // new StraightFlush(),
                // new FourOfAKind(),
                new FullHouse(),
                // new Flush(),
                // new Straight(),
                new ThreeOfAKind(),
                new TwoPairs(),
                new OnePair(),
                new HighCard()
            };

            var result = new EvaluatedHand();
            
            foreach (var rule in listRules)
            {
                var evaluationResult = rule.Check(playerHand, tableCards, isJokerGame);
                
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
        
        public static void SortByRankAscending(List<Card> cards)
        {
            for (var i = 0; i < cards.Count - 1; i++)
            {
                for (var j = i + 1; j < cards.Count; j++)
                {
                    if (cards[i].Rank > cards[j].Rank)
                    {
                        var tempCard = cards[i];
                        cards[i] = cards[j];
                        cards[j] = tempCard;
                    }
                }
            }
        }
        
        public static void SortByRankDescending(List<Card> cards)
        {
            for (var i = 0; i < cards.Count - 1; i++)
            {
                for (var j = i + 1; j < cards.Count; j++)
                {
                    if (cards[i].Rank < cards[j].Rank)
                    {
                        var tempCard = cards[i];
                        cards[i] = cards[j];
                        cards[j] = tempCard;
                    }
                }
            }
        }
    }
}