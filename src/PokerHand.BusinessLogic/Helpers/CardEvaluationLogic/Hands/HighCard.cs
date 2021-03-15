using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Hands
{
    public class HighCard : IRules
    {
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult
            {
                IsWinningHand = true,
                Hand = new Hand
                {
                    Cards = new List<Card>(5),
                    HandType = HandType.HighCard,
                    Value = 0
                }
            };

            var allCards = playerHand
                .Concat(tableCards)
                .OrderByDescending(c => (int)c.Rank)
                .ToList();

            for (var index = 0; index < 5; index++)
            {
                result.Hand.Cards.Add(allCards[index]);
                result.Hand.Value += (int)allCards[index].Rank;
            }
            
            return result;
        }
    }
}