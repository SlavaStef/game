using System;
using System.Collections.Generic;
using System.Linq;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class Deck
    {
        private const int MaxCardNumber = 52;
        private const int MaxCardRankNumber = 13;
        private const int MaxSuitTypeNumber = 4;
        private List<Card> Cards;

        public Deck()
        {
            Cards = GetShuffledDeck();
        }

        public List<Card> GetRandomCardsFromDeck(int numberOfCards)
        {
            var resultCards = new List<Card>();
            var random = new Random();

            for (var i = 0; i < numberOfCards; i++)
            {
                var cardFromDeck = Cards[random.Next(0, Cards.Count-1)];
                resultCards.Add(cardFromDeck);
                Cards.Remove(cardFromDeck);
            }

            return resultCards;
        }
        
        private static List<Card> GetShuffledDeck()
        {
            var deck = CreateDeck();

            var shuffledDeck = deck.OrderBy(x => Guid.NewGuid()).ToList();

            return shuffledDeck;
        }

        private static IEnumerable<Card> CreateDeck()
        {
            var newDeck = new List<Card>();

            foreach (int rank in Enum.GetValues(typeof(CardRankNumber)))
            {
                foreach (int suit in Enum.GetValues(typeof(SuitTypeNumber)))
                {
                    var newCard = new Card
                    {
                        Rank = Enum.Parse<CardRankNumber>(rank.ToString()),
                        Suit = Enum.Parse<SuitTypeNumber>(suit.ToString())
                    };
                    
                    newDeck.Add(newCard);
                }
            }

            return newDeck;
        }
    }
}