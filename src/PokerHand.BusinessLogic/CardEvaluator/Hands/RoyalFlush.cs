using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class RoyalFlush : IRules
    {
        private const int Rate = 200000;
        
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            return null;
        }
    }
}
