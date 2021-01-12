using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class Flush : IRules
    {
        private const int Rate = 1300;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            totalCards = new List<Card>(5);
            value = 0;
            var isFlush = false;
            handType = HandType.None;
            
            if (Check(allCards, isJokerGame, out List<Card> cards))
            {
                if (cards.Count > 4)
                {
                    foreach (var card in cards)
                        value += (int)card.Rank;

                    totalCards = cards;
                    value *= Rate;
                    isFlush = true;
                    handType = HandType.Flush;
                }
            }
            
            return isFlush;
        }

        public bool Check(List<Card> allCards, bool isJokerGame, out List<Card> cards)
        {
            cards = new List<Card>();
            var result = false;
            var numberOfJokers = allCards.FindAll(card => CardRankType.Joker == card.Rank).Count;

            FindAndTransformJoker(allCards, isJokerGame, numberOfJokers);
            result = FindFlush(allCards, cards, result);

            return result;
        }

        private bool FindFlush(List<Card> allCards, List<Card> cards, bool result)
        {
            for (var i = 0; i < 3; i++)
            {
                var numberOfSuits = allCards.FindAll(c => c.Suit == allCards[i].Suit).Count;
                if (numberOfSuits >= 5)
                {
                    foreach (var card in allCards)
                        if (card.Suit == allCards[i].Suit)
                            cards.Add(card);
                    result = true;
                    break;
                }
            }

            return result;
        }

        private void FindAndTransformJoker(List<Card> allCards, bool isJokerGame, int numberOfJokers)
        {
            if (!isJokerGame) return;
            
            foreach (var card in allCards)
            {
                if (numberOfJokers == 1)
                {
                    if (allCards.FindAll(c => c.Suit == card.Suit).Count == 4)
                        foreach (var currentCard in allCards)
                            if (currentCard.Rank == CardRankType.Joker)
                                currentCard.Suit = card.Suit;
                    break;
                }
                else
                {
                    if (allCards.FindAll(c => c.Suit == card.Suit).Count == 3)
                        foreach (var currentCard in allCards)
                            if (currentCard.Rank == CardRankType.Joker)
                                currentCard.Suit = card.Suit;
                    break;
                }
            }
        }
    }
}
