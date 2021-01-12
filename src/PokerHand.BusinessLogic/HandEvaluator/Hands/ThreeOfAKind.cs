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

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            JokerCheck(allCards, GetNumberOfJokers(isJokerGame, allCards));
            
            totalCards = new List<Card>(3);
            value = 0;
            var isThreeOfAKind = false;
            
            foreach (var card in allCards)
            {
                totalCards = allCards.Where(c => c.Rank == card.Rank).ToList();
                if (totalCards.Count == 3)
                {
                    value += (int)card.Rank * 3;
                    isThreeOfAKind = true;
                    break;
                }
            }

            if (isThreeOfAKind)
            {
                value *= Rate;
                handType = HandType.ThreeOfAKind;
            }
            else
            {
                value = 0;
                handType = HandType.None;
            }
            
            return isThreeOfAKind;
        }

        private int GetNumberOfJokers(bool isJokerGame, List<Card> cards)
        {
            if (!isJokerGame)
                return 0;

            return cards.Where(c => c.Rank == CardRankType.Joker).Select(c => c).Count();
        }

        private void JokerCheck(List<Card> cards, int numberOfJokers)
        {
            if (numberOfJokers == 1)
                CheckOneJoker(cards);
            else if (numberOfJokers == 2)
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
    }
}
