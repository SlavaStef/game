using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class OnePair : IRules
    {
        private const int Rate = 5;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            if (isJokerGame)
            {
                foreach (var card in allCards.Where(card => card.Rank == CardRankType.Joker))
                {
                    card.Rank = (CardRankType)GetMaxCardValue(allCards);
                    card.WasJoker = true;
                }
            }

            finalCardsList = new List<Card>(5);
            var isOnePair = false;
            value = 0;
            
            // There may be only one pair
            foreach (var card in allCards)
            {
                if (allCards.Count(c => c.Rank == card.Rank) == 2)
                {
                    // Add the pair of cards from the list of all cards to the final list
                    var cardsToAdd = allCards.Where(c => c.Rank == card.Rank).ToList();

                    foreach (var cardToAdd in cardsToAdd.Where(cardToAdd => cardToAdd.WasJoker))
                        cardToAdd.Rank = CardRankType.Joker;
                    
                    finalCardsList.AddRange(cardsToAdd);

                    // Remove pair cards from the list of all cards
                    
                    foreach (var c in cardsToAdd)
                        allCards.Remove(c);

                    AddCardsSortedByValue(finalCardsList, allCards);
                    
                    value += (int)card.Rank * 2;
                    isOnePair = true;
                    break;
                }
            }

            if (isOnePair)
            {
                value *= Rate;
                handType = HandType.OnePair;
            }
            else
            {
                value = 0;
                handType = HandType.None;
                finalCardsList = null;
            }

            return isOnePair;
        }

        private void AddCardsSortedByValue(List<Card> finalCardsList, List<Card> allCards)
        {
            CardEvaluator.CardEvaluator.SortByRankDescending(allCards);

            for (var i = 0; i < 3; i++)
                finalCardsList.Add(allCards[i]);
        }

        private int GetMaxCardValue(List<Card> cards)
        {
            var  maxValue = 0;
            
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank && card.Rank != CardRankType.Joker)
                {
                    maxValue = (int)card.Rank;
                }
            }
            
            return maxValue;
        }
    }
}