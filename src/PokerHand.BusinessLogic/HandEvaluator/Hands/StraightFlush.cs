using System.Collections.Generic;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class StraightFlush : IRules
    {
        private const int Rate = 180000;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
        {
            var isStraightFlush = false;
            var straightCheck = new Straight();
            var flushCheck = new Flush();
            value = 0;
            totalCards = new List<Card>();

        
            if (straightCheck.IsStraight(playerHand, tableCards, isJokerGame, out List<Card> newCards) 
                && flushCheck.Check(newCards, isJokerGame, out List<Card> newFlushCards))
            {
                value = 0;
                if (newFlushCards.Count > 4)
                {
                    foreach (Card card in newFlushCards)
                        value += (int)card.Rank;

                    totalCards = newFlushCards;
                    isStraightFlush = true;
                }
            }

            if (isStraightFlush)
            {
                value *= Rate;
                handType = HandType.StraightFlush;
            }
            else
            {
                value = 0;
                handType = HandType.StraightFlush;
            }
        
            return isStraightFlush;
        }
    }
}
