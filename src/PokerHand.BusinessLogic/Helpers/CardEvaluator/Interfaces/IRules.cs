using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluator.Interfaces
{
    public interface IRules
    {
        EvaluationResult Check(List<Card> playerHand, List<Card> tableCards);
    }
}