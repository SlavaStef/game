using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class TwoPairs : IRules
    {
        private const int Rate = 17;

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
            value = 0;
            var lastValue = -1;
            var numberOfPairs = 0;

            for (var i = 0; i < 2; i++)
            {
                foreach (var card in allCards)
                {
                    if (allCards.Count(c => c.Rank == card.Rank) == 2)
                    {
                        value += (int)card.Rank * 2;
                        lastValue = (int)card.Rank;
                        numberOfPairs++;
                        
                        var cardsToAdd = allCards.Where(c => c.Rank == card.Rank).ToArray();
                        
                        foreach (var cardToAdd in cardsToAdd.Where(cardToAdd => cardToAdd.WasJoker))
                            cardToAdd.Rank = CardRankType.Joker;
                        
                        finalCardsList.AddRange(cardsToAdd);
                        
                        allCards.RemoveAll(c => cardsToAdd.Contains(c));
                        break;
                    }
                }
                CardEvaluator.SortByRankDescending(finalCardsList);
            }

            var isTwoPairs = false;
            
            if (numberOfPairs == 2)
            {
                isTwoPairs = true;
                value *= Rate;
                handType = HandType.TwoPairs;
                
                // Add the fifth card
                AddCardsSortedByValue(finalCardsList, allCards);
            }
            else
            {
                value = 0;
                handType = HandType.None;
                finalCardsList = null;
            }

            return isTwoPairs;
        }
        
        private void AddCardsSortedByValue(List<Card> finalCardsList, List<Card> allCards)
        {
            CardEvaluator.SortByRankDescending(allCards);
            
            finalCardsList.Add(allCards[0]);
        }
        
        private int GetMaxCardValue(List<Card> cards)
        {
            var maxValue = 0;
            
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank && cards.FindAll(c => c.Rank == card.Rank).Count < 2)
                {
                    if (card.Rank != CardRankType.Joker)
                        maxValue = (int)card.Rank;
                }
            }
            
            return maxValue;
        }
    }
}