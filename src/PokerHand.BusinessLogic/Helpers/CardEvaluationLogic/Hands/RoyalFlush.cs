using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Hands
{
    public class RoyalFlush : IRules
    {
        private const int Rate = 200000;
        
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();

            var straightFlushCheckResult = new StraightFlush().Check(playerHand, tableCards);

            switch (straightFlushCheckResult.IsWinningHand)
            {
                case true when straightFlushCheckResult.EvaluatedHand.Cards[0].Rank is CardRankType.Ace:
                {
                    result.IsWinningHand = true;
                    result.EvaluatedHand.HandType = HandType.RoyalFlush;
                    result.EvaluatedHand.Cards = straightFlushCheckResult.EvaluatedHand.Cards.ToList();

                    foreach (var card in result.EvaluatedHand.Cards)
                        result.EvaluatedHand.Value += (int) card.Rank * Rate;

                    return result;
                }
                case false:
                    return EvaluationResult(playerHand, tableCards);
            }

            return result;
        }

        private static EvaluationResult EvaluationResult(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            var allCards = playerHand.Concat(tableCards).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            switch (numberOfJokers)
            {
                case 1:
                    break;
                case 2:
                    break;
            }
            return result;
        }
    }
}