using System;
using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.BusinessLogic.Services
{
    public class DeckService : IDeckService
    {
        private const int MaxCardNumberTexasPoker = 52;
        private const int MaxCardNumberRoyalPoker = 20;
        private const int MaxCardNumberJokerPoker = 54;

        public Deck GetNewDeck(TableType tableType)
        {
            var orderedDeck = CreateNewDeck(tableType);
            var shuffledDeck = ShuffledDeck(orderedDeck);

            var deck = new Deck {Cards = shuffledDeck};

            return deck;
        }

        public List<Card> GetRandomCardsFromDeck(Deck deck, int numberOfCards)
        {
            if (numberOfCards > deck.Cards.Count || numberOfCards < 0)
                return null;
            
            var resultCards = new List<Card>(numberOfCards);
            var random = new Random();

            for (var i = 0; i < numberOfCards; i++)
            {
                var cardFromDeck = deck.Cards[random.Next(0, deck.Cards.Count - 1)];
                resultCards.Add(cardFromDeck);
                deck.Cards.Remove(cardFromDeck);
            }

            return resultCards;
        }

        #region Helpers

        private static List<Card> ShuffledDeck(IEnumerable<Card> orderedDeck)
        {
            var shuffledDeck = orderedDeck
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            return shuffledDeck;
        }

        private static IEnumerable<Card> CreateNewDeck(TableType tableType)
        {
            List<Card> newDeck;

            switch (tableType)
            {
                case TableType.TexasPoker:
                case TableType.SitAndGo:
                case TableType.DashPoker:
                case TableType.LawballPoker:
                    newDeck = GenerateDeck(MaxCardNumberTexasPoker);
                    break;
                case TableType.RoyalPoker:
                    newDeck = GenerateDeck(MaxCardNumberRoyalPoker);
                    break;
                case TableType.JokerPoker:
                    newDeck = GenerateDeck(MaxCardNumberJokerPoker);

                    newDeck.Add(new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red});
                    newDeck.Add(new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black});
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(tableType), tableType, "Wrong table type");
            }

            return newDeck;

            static List<Card> GenerateDeck(int maxCardsNumber)
            {
                var deck = new List<Card>(maxCardsNumber);
                var lowestRank = maxCardsNumber is MaxCardNumberRoyalPoker ? 10 : 2;

                for (var rankType = lowestRank; rankType < 15; rankType++)
                {
                    for (var suitType = 0; suitType < 4; suitType++)
                    {
                        var newCard = new Card
                        {
                            Rank = (CardRankType) rankType,
                            Suit = (CardSuitType) suitType
                        };

                        deck.Add(newCard);
                    }
                }

                return deck;
            }
        }

        #endregion
    }
}