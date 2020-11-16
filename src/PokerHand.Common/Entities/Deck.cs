using System.Collections.Generic;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class Deck
    {
        private const int MaxCardNumber = 52;
        private List<Card> Cards;

        public Deck()
        {
            Cards = GetShuffledDeck();
        }

        public bool RemoveFromDeck(Card card)
        {
            return Cards.Remove(card);
        }
        
        private List<Card> GetShuffledDeck()
        {
            List<Card> deck = CreateDeck();
            ShuffleCards(ref deck);

            return deck;
        }

        private List<Card> CreateDeck()
        {
            return new List<Card>();
        }

        private void ShuffleCards(ref List<Card> deck)
        {
            
        }
        
    }
}