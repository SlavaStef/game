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

            if (isJokerGame) 
                CheckJokers(allCards);

            finalCardsList = new List<Card>(5);
            value = 0;
            var isThreeOfAKind = false;
            
            foreach (var card in allCards)
            {
                var possibleThreeOfAKind = allCards.Where(c => c.Rank == card.Rank).ToList();
                if (possibleThreeOfAKind.Count == 3)
                {
                    isThreeOfAKind = true;
                    value += (int)card.Rank * 3;
                    
                    finalCardsList = possibleThreeOfAKind.ToList();

                    // Reset jokers
                    foreach (var cardToAdd in possibleThreeOfAKind.Where(cardToAdd => cardToAdd.WasJoker))
                        cardToAdd.Rank = CardRankType.Joker;
                    
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
        
        private void CheckJokers(List<Card> cards)
        {
            // If there is 1 joker -> check if there is a pair
            // If there are 2 jokers -> find a card with the highest value
            
            var numberOfJokers = cards.Count(c => c.Rank == CardRankType.Joker);
        
            if (numberOfJokers == 1)
                CheckWithOneJoker(cards);
            else
                CheckWithTwoJokers(cards);
        }
        
        private void CheckWithOneJoker(List<Card> cards)
        {
            foreach (var card in cards)
            {
                if (cards.FindAll(c => c.Rank == card.Rank).Count == 2)
                {
                    var joker = cards.First(c => c.Rank == CardRankType.Joker);
                    
                    joker.Rank = card.Rank;
                    joker.WasJoker = true;
                    return;
                }
            }
        }
        
        private void CheckWithTwoJokers(List<Card> cards)
        {
            foreach (var card in cards.Where(c => c.Rank == CardRankType.Joker))
            {
                card.Rank = (CardRankType)GetMaxValue(cards);
                card.WasJoker = true;
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
