using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic
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

            foreach (var card in allCards.TakeWhile(card => result.Hand.Cards.Count is not 5))
            {
                result.Hand.Cards.Add(card);
                result.Hand.Value += (int)card.Rank;
            }
            
            return result;
        }
    }
}