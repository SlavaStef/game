using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface ICardEvaluationService
    {
        List<SidePot> CalculateSidePotsWinners(Table table);
        Player EvaluatePlayerHand(List<Card> communityCards, Player player);
    }
}