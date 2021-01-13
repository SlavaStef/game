using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Hands;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.HandEvaluator
{
    public static class CardEvaluator
    {
        public static List<Player> DefineWinners(List<Card> communityCards, List<Player> players, bool isJokerGame)
        {
            var currentMaxValue = 0;
            
            foreach (var player in players)
            {
                Check(player.PocketCards, communityCards, isJokerGame, out int handValue, out HandType handType, out List<Card> resultCards);

                player.Hand = handType;
                player.HandValue = handValue;
                player.HandCombinationCards = resultCards;

                if (player.HandValue > currentMaxValue)
                    currentMaxValue = player.HandValue;
            }

            var winners = players.Where(p => p.HandValue == currentMaxValue).ToList();

            return winners;
        }
        
        private static bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int handValue, out HandType handType, out List<Card> resultCards)
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
            
            handValue = 0;
            handType = HandType.None;
            resultCards = new List<Card>(7);
            var result = false;

            foreach (var rule in listRules)
            {
                result = rule.Check(playerHand, tableCards, isJokerGame, out handValue, out handType, out resultCards);
                if (result) break;
            }

            return result;
        }
        
        public static void SortByRank(List<Card> cards)
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
    // public class HandEvaluator
    // {
    //     private IRules _currentRules;
    //     private readonly List<IRules> _listRules;
    //     private bool _result;
    //
    //     public HandEvaluator()
    //     {
    //         _listRules = new List<IRules>
    //         {
    //             new RoyalFlush(),
    //             new StraightFlush(),
    //             new FourOfAKind(),
    //             new FullHouse(),
    //             new Flush(),
    //             new Straight(),
    //             new ThreeOfAKind(),
    //             new TwoPairs(),
    //             new OnePair(),
    //             new HighCard()
    //         };
    //     }
    //
    //     public List<Player> DefineWinners(List<Card> communityCards, List<Player> players, bool isJokerGame)
    //     {
    //         var currentMaxValue = 0;
    //         
    //         foreach (var player in players)
    //         {
    //             Check(player.PocketCards, communityCards, isJokerGame, out int handValue, out HandType handType, out List<Card> resultCards);
    //
    //             player.Hand = handType;
    //             player.HandValue = handValue;
    //             player.HandCombinationCards = resultCards;
    //
    //             if (player.HandValue > currentMaxValue)
    //                 currentMaxValue = player.HandValue;
    //         }
    //
    //         var winners = players.Where(p => p.HandValue == currentMaxValue).ToList();
    //
    //         return winners;
    //     }
    //
    //     public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int handValue, out HandType handType, out List<Card> resultCards)
    //     {
    //         handValue = 0;
    //         handType = HandType.None;
    //         resultCards = new List<Card>(7);
    //
    //         foreach (var rule in _listRules)
    //         {
    //             _currentRules = rule;
    //             _result = _currentRules.Check(playerHand, tableCards, isJokerGame, out handValue, out handType, out resultCards);
    //             if (_result) break;
    //         }
    //
    //         return _result;
    //     }
    // }
}