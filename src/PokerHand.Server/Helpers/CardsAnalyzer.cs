using System.Collections.Generic;
using System.Linq;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.Server.Helpers
{
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