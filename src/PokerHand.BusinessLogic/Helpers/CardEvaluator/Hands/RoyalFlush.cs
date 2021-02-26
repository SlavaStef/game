using System.Collections.Generic;
using PokerHand.BusinessLogic.Helpers.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluator.Hands
{
    public class RoyalFlush : IRules
    {
        private const int Rate = 200000;
        
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            return null;
        }
    }
}
