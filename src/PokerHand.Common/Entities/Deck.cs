using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<Card> resultCards = new List<Card>();
            Random random = new Random();

            for (int i = 0; i < numberOfCards; i++)
            {
                Card cardFromDeck = Cards[random.Next(0, Cards.Count-1)];
                resultCards.Add(cardFromDeck);
                Cards.Remove(cardFromDeck);
            }

            return resultCards;
        }
        
        private List<Card> GetShuffledDeck()
        {
            List<Card> deck = CreateDeck();

            List<Card> shuffledDeck = deck.OrderBy(x => Guid.NewGuid()).ToList();

            return shuffledDeck;
        }

        private List<Card> CreateDeck()
        {
            List<Card> newDeck = new List<Card>();

            foreach (int rank in Enum.GetValues(typeof(CardRankNumber)))
            {
                foreach (int suit in Enum.GetValues(typeof(SuitTypeNumber)))
                {
                    Card newCard = new Card
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