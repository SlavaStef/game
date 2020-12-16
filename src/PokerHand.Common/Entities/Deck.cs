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

        public Deck() { }
        public Deck(TableType tableType)
        {
            Cards = GetShuffledDeck(tableType);
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
        
        private static List<Card> GetShuffledDeck(TableType tableType)
        {
            var deck = CreateDeck(tableType);

            var shuffledDeck = deck.OrderBy(x => Guid.NewGuid()).ToList();

            return shuffledDeck;
        }

        private static IEnumerable<Card> CreateDeck(TableType tableType)
        {
            var newDeck = new List<Card>();

            switch (tableType)
            {
                case TableType.TexasPoker:
                case TableType.SitAndGo:
                case TableType.DashPoker:
                case TableType.LawballPoker:
                    foreach (int rank in Enum.GetValues(typeof(CardRankType)))
                    {
                        foreach (int suit in Enum.GetValues(typeof(CardSuitType)))
                        {
                            var newCard = new Card
                            {
                                Rank = Enum.Parse<CardRankType>(rank.ToString()),
                                Suit = Enum.Parse<CardSuitType>(suit.ToString())
                            };
                    
                            newDeck.Add(newCard);
                        }
                    }
                    break;
                case TableType.RoyalPoker:
                    foreach (int rank in Enum.GetValues(typeof(RoyalPokerCardRankType)))
                    {
                        foreach (int suit in Enum.GetValues(typeof(CardSuitType)))
                        {
                            var newCard = new Card
                            {
                                Rank = Enum.Parse<CardRankType>(rank.ToString()),
                                Suit = Enum.Parse<CardSuitType>(suit.ToString())
                            };
                    
                            newDeck.Add(newCard);
                        }
                    }
                    break;
                case TableType.JokerPoker:
                    foreach (int rank in Enum.GetValues(typeof(JokerPokerCardRankType)))
                    { 
                        foreach (int suit in Enum.GetValues(typeof(CardSuitType)))
                        {
                            var newCard = new Card
                            {
                                Rank = Enum.Parse<CardRankType>(rank.ToString()),
                                Suit = Enum.Parse<CardSuitType>(suit.ToString())
                            };
                    
                            newDeck.Add(newCard);
                        }
                    }
                    break;
  
                default:
                    throw new ArgumentOutOfRangeException(nameof(tableType), tableType, null);
            }
            
            newDeck = newDeck.Where(card => card.Rank != CardRankType.Joker).ToList(); 

            return newDeck;
        }
    }
}