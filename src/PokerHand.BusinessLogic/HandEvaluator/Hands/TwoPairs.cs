using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class TwoPairs : IRules
    {
        private const int Rate = 17;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            if (isJokerGame)
            {
                foreach (var card in allCards)
                    if (card.Rank == CardRankType.Joker)
                        card.Rank = (CardRankType)GetMaxCardValue(allCards);
            }

            totalCards = new List<Card>(4);
            value = 0;
            var lastValue = -1;
            var counter = 0;

            for (var i = 0; i < 2; i++)
            {
                foreach (var card in allCards)
                {
                    if (allCards.FindAll(c => c.Rank == card.Rank).Count == 2)
                    {
                        value += (int)card.Rank * 2;
                        lastValue = (int)card.Rank;
                        counter++;
                        
                        var cards = allCards.Where(c => c.Rank == card.Rank).ToArray();
                        totalCards.AddRange(cards);
                        
                        allCards.RemoveAll(c => (int)c.Rank == lastValue);
                        break;
                    }
                }
            }

            var isTwoPairs = false;
            
            if (counter == 2)
            {
                isTwoPairs = true;
                value *= Rate;
                handType = HandType.TwoPairs;
            }
            else
            {
                value = 0;
                handType = HandType.None;
            }

            return isTwoPairs;
        }
        
        private int GetMaxCardValue(List<Card> cards)
        {
            var maxValue = 0;
            
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank && cards.FindAll(c => c.Rank == card.Rank).Count < 2)
                {
                    if (card.Rank != CardRankType.Joker)
                        maxValue = (int)card.Rank;
                }
            }
            
            return maxValue;
        }
    }
}