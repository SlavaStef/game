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

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            finalCardsList = new List<Card>(5);
            value = 0;
            handType = HandType.None;

            var isFlush = IsFlush(playerHand, tableCards, isJokerGame, out List<Card> finalCards);
            
            if (isFlush)
            {
                foreach (var card in finalCards)
                    value += (int)card.Rank;

                finalCardsList = finalCards;
                value *= Rate;
                handType = HandType.Flush;
            }
            
            return isFlush;
        }

        public bool IsFlush(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out List<Card> finalCards)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            finalCards = new List<Card>(7);

            if(isJokerGame)
                FindAndTransformJoker(allCards);

            var isFlush = false;
            
            for (var i = 0; i < 3; i++)
            {
                var numberOfCurrentSuitCards = allCards.FindAll(c => c.Suit == allCards[i].Suit).Count;
                
                if (numberOfCurrentSuitCards >= 5)
                {
                    foreach (var card in allCards)
                        if (card.Suit == allCards[i].Suit)
                            finalCards.Add(card);

                    CardEvaluator.SortByRankAscending(finalCards);

                    // Delete redundant cards if there are more than 5 cards of the same suit
                    while (finalCards.Count > 5)
                        finalCards.Remove(finalCards[0]);
                    
                    isFlush = true;
                    break;
                }
            }

            return isFlush;
        }
        
        private void FindAndTransformJoker(List<Card> allCards)
        {
            var numberOfJokers = allCards.Count(card => CardRankType.Joker == card.Rank);
            
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
