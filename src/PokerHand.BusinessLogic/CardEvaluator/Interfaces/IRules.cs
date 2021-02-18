using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.CardEvaluator.Interfaces
{
    public interface IRules
    {
        //bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList);

        EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame);
    }
}