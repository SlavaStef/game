using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class HighCard : IRules
    {
        private const int Rate = 1;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var allCards = playerHand.ToList();
            var highestValue = 0;
            var highestCard = new Card();
            
            foreach (var card in allCards)
            {
                if ((int)card.Rank > highestValue) 
                {
                    highestValue = (int)card.Rank;
                    highestCard = card;
                }
            }

            finalCardsList = new List<Card>(1)
            {
                highestCard
            };
            
            value = highestValue * Rate;
            handType = HandType.HighCard;

            return true;
        }
    }
}