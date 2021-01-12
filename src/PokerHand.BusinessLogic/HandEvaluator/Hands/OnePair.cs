using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class OnePair : IRules
    {
        private const int Rate = 5;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            if (isJokerGame)
            {
                foreach (var card in allCards)
                    if (card.Rank == CardRankType.Joker)
                        card.Rank = (CardRankType)GetMaxCardValue(allCards);
            }

            totalCards = new List<Card>(2);
            var currentHighestRank = 0;
            var isOnePair = false;
            var highestValue = 0;
            
            foreach (var card in allCards)
            {
                totalCards = allCards.Where(c => c.Rank == card.Rank).ToList();
                if (totalCards.Count == 2 && currentHighestRank < (int)card.Rank)
                {
                    currentHighestRank = (int) card.Rank;
                    highestValue += (int)card.Rank * 2;
                    isOnePair = true;
                    break;
                }
            }

            if (isOnePair)
            {
                value = highestValue * Rate;
                handType = HandType.OnePair;
            }
            else
            {
                value = 0;
                handType = HandType.None;
            }

            return isOnePair;
        }

        private int GetMaxCardValue(List<Card> cards)
        {
            var  maxValue = 0;
            
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank)
                {
                    if (card.Rank != CardRankType.Joker)
                        maxValue = (int)card.Rank;
                }
            }
            
            return maxValue;
        }
    }
}