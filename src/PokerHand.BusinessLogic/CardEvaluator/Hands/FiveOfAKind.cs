using System.Collections.Generic;
using Microsoft.AspNetCore.Rewrite;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class FiveOfAKind : IRules
    {
        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType,
            out List<Card> finalCardsList)
        {
            throw new System.NotImplementedException();
        }
    }
}