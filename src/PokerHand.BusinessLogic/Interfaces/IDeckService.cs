using System.Collections.Generic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IDeckService
    {
        Deck GetNewDeck(TableType tableType);
        List<Card> GetRandomCardsFromDeck(Deck deck, int numberOfCards);
    }
}