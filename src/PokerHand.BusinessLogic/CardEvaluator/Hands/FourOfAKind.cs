using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class FourOfAKind : IRules
    {
        private const int Rate = 60000;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            var isFourOfAKind = false;
            value = 0;
            finalCardsList = new List<Card>();
            
            JokerCheck(isJokerGame, allCards);

            handType = HandType.None;
            CardEvaluator.SortByRank(allCards);
            
            foreach (var card in allCards)
            {
                var tempCards = allCards.Where(c => c.Rank == card.Rank).ToList();
                if (tempCards.Count == 4)
                {
                    var cardsToAdd = allCards.Where(c => c.Rank == card.Rank).ToList();
                    finalCardsList.AddRange(cardsToAdd);

                    foreach (var c in cardsToAdd)
                        allCards.Remove(c);

                    finalCardsList.Add(allCards[allCards.Count - 1]);
                    
                    value += (int)card.Rank * 4;
                    isFourOfAKind = true;
                    value *= Rate;
                    handType = HandType.FourOfAKind;
                    break;
                }
            }

            return isFourOfAKind;
        }
        
        private void JokerCheck(bool isJokerGame, List<Card> allCards)
        {
            if (!isJokerGame) return;

            var numberOfJokers = 0;
            
            foreach (var card in allCards)
                if (card.Rank == CardRankType.Joker)
                    numberOfJokers++;

            if (numberOfJokers == 1)
                OneJokerCheck(allCards);
            else if (numberOfJokers == 2)
                TwoJokerCheck(allCards);
        }

        private void TwoJokerCheck(List<Card> allCards)
        {
            foreach (var card in allCards)
            {
                if (allCards.FindAll(c => c.Rank == card.Rank).Count == 2)
                {
                    if (card.Rank == CardRankType.Joker) continue;
                    
                    foreach (var tempCard in allCards)
                    {
                        if (tempCard.Rank == CardRankType.Joker)
                            tempCard.Rank = card.Rank;
                    }
                }
                OneJokerCheck(allCards);
            }
        }

        private void OneJokerCheck(List<Card> allCards)
        {
            foreach (var card in allCards)
            {
                if (allCards.FindAll(c => c.Rank == card.Rank).Count == 3)
                {
                    foreach (var tempCard in allCards)
                    {
                        if (tempCard.Rank == CardRankType.Joker)
                        {
                            tempCard.Rank = card.Rank;
                            break;
                        }
                    }
                }
            }
        }
    }
}
