using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class HighCard : IRules
    {
        private const int Rate = 1;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var result = new EvaluationResult()
            {
                IsWinningHand = true
            };

            var highestValue = playerHand
                .Select(c => (int) c.Rank)
                .Max();

            var highestCard = playerHand
                .First(c => (int) c.Rank == highestValue);

            result.EvaluatedHand.Cards = new List<Card>(1)
            {
                highestCard
            };
            
            result.EvaluatedHand.Value = highestValue * Rate;
            result.EvaluatedHand.HandType = HandType.HighCard;

            return result;
        }
    }
}