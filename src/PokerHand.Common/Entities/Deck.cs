using System;
using System.Collections.Generic;
using System.Linq;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.Common.Entities
{
    public class Deck
    {
        private const int MaxCardNumberTexasPoker = 52;
        private const int MaxCardNumberRoyalPoker = 20;
        private const int MaxCardNumberJokerPoker = 54;
        private readonly List<Card> _cards;
        
        public Deck(TableType tableType)
        {
            _cards = GetShuffledDeck(tableType);
        }

        public List<Card> GetRandomCardsFromDeck(int numberOfCards)
        {
            var resultCards = new List<Card>(numberOfCards);
            var random = new Random();

            for (var i = 0; i < numberOfCards; i++)
            {
                var cardFromDeck = _cards[random.Next(0, _cards.Count-1)];
                resultCards.Add(cardFromDeck);
                _cards.Remove(cardFromDeck);
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
            List<Card> newDeck;

            switch (tableType)
            {
                case TableType.TexasPoker:
                case TableType.SitAndGo:
                case TableType.DashPoker:
                case TableType.LawballPoker:
                    newDeck = new List<Card>(MaxCardNumberTexasPoker);

                    for (var rankType = 2; rankType < 15; rankType++)
                    {
                        for (var suitType = 0; suitType < 4; suitType++)
                        {
                            var newCard = new Card
                            {
                                Rank = (CardRankType) rankType,
                                Suit = (CardSuitType) suitType
                            };
                            
                            newDeck.Add(newCard);
                        }
                    }
                    
                    break;
                case TableType.RoyalPoker:
                    newDeck = new List<Card>(MaxCardNumberRoyalPoker);
                    
                    for (var rankType = 10; rankType < 15; rankType++)
                    {
                        for (var suitType = 0; suitType < 4; suitType++)
                        {
                            var newCard = new Card
                            {
                                Rank = (CardRankType) rankType,
                                Suit = (CardSuitType) suitType
                            };
                            
                            newDeck.Add(newCard);
                        }
                    }
                    
                    break;
                case TableType.JokerPoker:
                    newDeck = new List<Card>(MaxCardNumberJokerPoker);

                    for (var rankType = 2; rankType < 15; rankType++)
                    {
                        for (var suitType = 0; suitType < 4; suitType++)
                        {
                            var newCard = new Card
                            {
                                Rank = (CardRankType) rankType,
                                Suit = (CardSuitType) suitType
                            };

                            newDeck.Add(newCard);
                        }
                    }

                    newDeck.Add(new Card { Rank = CardRankType.Joker, Suit = CardSuitType.Red });
                    newDeck.Add(new Card{ Rank = CardRankType.Joker, Suit = CardSuitType.Black });
                    break;
  
                default:
                    throw new ArgumentOutOfRangeException(nameof(tableType), tableType, null);
            }
            
            return newDeck;
        }
    }
}