using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluator.Hands
{
    public class HighCard : IRules
    {
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult
            {
                IsWinningHand = true,
                EvaluatedHand = new EvaluatedHand
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
                result.EvaluatedHand.Cards.Add(allCards[index]);
                result.EvaluatedHand.Value += (int)allCards[index].Rank;
            }
            
            return result;
        }
    }
}