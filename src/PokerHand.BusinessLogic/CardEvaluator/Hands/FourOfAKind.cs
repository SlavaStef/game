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
            
            if (isJokerGame)
                CheckJokers(allCards);

            handType = HandType.None;
            CardEvaluator.SortByRankAscending(allCards);
            
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
        
        private void CheckJokers(List<Card> allCards)
        {
            var numberOfJokers = allCards.Count(card => card.Rank == CardRankType.Joker);

            switch (numberOfJokers)
            {
                case 1:
                    CheckOneJoker(allCards);
                    break;
                case 2:
                    CheckTwoJokers(allCards);
                    break;
            }
        }

        private void CheckOneJoker(List<Card> allCards)
        {
            foreach (var card in allCards.Where(card => allCards.FindAll(c => c.Rank == card.Rank).Count == 3))
            {
                foreach (var tempCard in allCards.Where(tempCard => tempCard.Rank == CardRankType.Joker))
                {
                    tempCard.Rank = card.Rank;
                    tempCard.WasJoker = true;
                    break;
                }
            }
        }
        
        private void CheckTwoJokers(List<Card> allCards)
        {
            foreach (var card in allCards)
            {
                if (allCards.FindAll(c => c.Rank == card.Rank).Count == 2)
                {
                    if (card.Rank == CardRankType.Joker) continue;

                    foreach (var tempCard in allCards.Where(tempCard => tempCard.Rank == CardRankType.Joker))
                    {
                        tempCard.Rank = card.Rank;
                        tempCard.WasJoker = true;
                    }
                }
                CheckOneJoker(allCards);
            }
        }
    }
}
