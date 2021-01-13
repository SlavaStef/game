using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class ThreeOfAKind : IRules
    {
        private const int Rate = 170;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            if(isJokerGame)
                JokerCheck(allCards);
            
            finalCardsList = new List<Card>(5);
            value = 0;
            var isThreeOfAKind = false;
            
            foreach (var card in allCards)
            {
                var possibleThreeOfAKind = allCards.Where(c => c.Rank == card.Rank).ToList();
                if (possibleThreeOfAKind.Count == 3)
                {
                    value += (int)card.Rank * 3;
                    isThreeOfAKind = true;

                    finalCardsList = possibleThreeOfAKind.ToList();

                    foreach (var cardToRemove in possibleThreeOfAKind) 
                        allCards.Remove(cardToRemove);
                    
                    break;
                }
            }

            if (isThreeOfAKind)
            {
                value *= Rate;
                handType = HandType.ThreeOfAKind;
                
                AddCardsSortedByValue(finalCardsList, allCards);
            }
            else
            {
                value = 0;
                handType = HandType.None;
                finalCardsList = null;
            }
            
            return isThreeOfAKind;
        }
        
        private void JokerCheck(List<Card> cards)
        {
            var numberOfJokers = cards
                .Where(c => c.Rank == CardRankType.Joker)
                .Select(c => c)
                .Count();
            
            if (numberOfJokers == 1)
                CheckOneJoker(cards);
            else
                CheckTwoJokers(cards);
        }
        
        private void CheckOneJoker(List<Card> cards)
        {
            foreach (var card in cards)
            {
                if (card.Rank == CardRankType.Joker)
                    card.Rank = (CardRankType)GetMaxValue(cards);
            }
        }

        private void CheckTwoJokers(List<Card> cards)
        {
            foreach (var card in cards)
            {
                if (cards.FindAll(c => c.Rank == card.Rank).Count == 2)
                {
                    foreach (var currentCard in cards
                        .Where(currentCard => currentCard.Rank == CardRankType.Joker)
                        .Select(currentCard => currentCard))
                    {
                        currentCard.Rank = (CardRankType)GetMaxValue(cards);
                        break;
                    }
                }
            }
        }

        private int GetMaxValue(List<Card> cards)
        {
            var maxValue = 0;
        
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank)
                {
                    if (card.Rank != CardRankType.Joker)
                        maxValue = (int)card.Rank;
                }
            }
        
            return maxValue;
        }
        
        private void AddCardsSortedByValue(List<Card> finalCardsList, List<Card> allCards)
        {
            CardEvaluator.SortByRankDescending(allCards);
            
            for(var i = 0; i < 2; i++)
                finalCardsList.Add(allCards[i]);
        }
        
    }
}
