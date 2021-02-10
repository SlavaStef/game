using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Hands;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
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
                var (handValue, handType, bestCombination) = FindCombination(player.PocketCards, communityCards, isJokerGame);

                player.Hand = handType;
                player.HandValue = handValue;
                player.HandCombinationCards = bestCombination;

                // if (player.HandValue > currentMaxValue)
                //     currentMaxValue = player.HandValue;
            }

            // var winners = players.Where(p => p.HandValue == currentMaxValue).ToList();
            //
            // return winners;

            return players;
        }
        
        private static (int handValue, HandType handType, List<Card> bestCombination) FindCombination(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var listRules = new List<IRules>
            {
                new RoyalFlush(),
                new StraightFlush(),
                new FourOfAKind(),
                new FullHouse(),
                new Flush(),
                new Straight(),
                new ThreeOfAKind(),
                new TwoPairs(),
                new OnePair(),
                new HighCard()
            };
            
            var handValue = 0;
            var handType = HandType.None;
            var bestCombination = new List<Card>(7);
            
            foreach (var rule in listRules)
            {
                if (rule.Check(playerHand, tableCards, isJokerGame, out handValue, out handType, out bestCombination)) 
                    break;
            }

            return (handValue, handType, bestCombination);
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