using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class FullHouse : IRules
    {
        private const int Rate = 6700;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
           
            JokerCheck(isJokerGame, allCards);

            var lastValue = -1;
            var counter = 0;
            value = 0;
            finalCardsList = new List<Card>(5);
            
            CheckOnThreeCard(ref value, allCards, ref lastValue, ref counter, ref finalCardsList);
            
            if (lastValue != -1) 
                allCards.RemoveAll(c => (int)c.Rank == lastValue);
            
            CheckOnTwoCards(ref value, allCards, ref counter, ref finalCardsList);

            var isFullHouse = false;
            handType = HandType.None;
            if (counter == 2)
            {
                isFullHouse = true;
                value *= Rate;
                handType = HandType.FullHouse;
            }
            else
            {
                value = 0;
                finalCardsList = null;
            }
                

            return isFullHouse;
        }

        private void JokerCheck(bool isJokerGame, List<Card> cards)
        {
            if (!isJokerGame) return;

            var numberOfJokers = cards.Where(card => card.Rank == CardRankType.Joker).Select(card => card).Count();

            if (numberOfJokers == 1)
                OneJokerCheck(cards);
            else
                TwoJokerCheck(cards);
        }

        private void OneJokerCheck(List<Card> cards)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 3) continue;
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 2)
                {
                    foreach (var card in cards)
                        if (card.Rank == CardRankType.Joker)
                            card.Rank = cards[i].Rank;
                    break;
                }
                
                foreach (var card in cards)
                    if (card.Rank == CardRankType.Joker)
                        card.Rank = (CardRankType)GetMaxValue(cards);
            }
        }
        
        private void TwoJokerCheck(List<Card> cards)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 3) continue;
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 2)
                {
                    foreach (var card in cards)
                        if (card.Rank == CardRankType.Joker)
                            card.Rank = cards[i].Rank;
                    break;
                }
                foreach (var card in cards)
                    if (card.Rank == CardRankType.Joker)
                        card.Rank = (CardRankType)GetMaxValue(cards);
            }
        }
        
        private int GetMaxValue(List<Card> cards)
        {
            var maxValue = 0;
            
            for (var i = 1; i < cards.Count; i++)
            {
                if (maxValue < (int)cards[i].Rank)
                {
                    if (cards[i].Rank != CardRankType.Joker)
                        maxValue = (int)cards[i].Rank;
                }
            }
            
            return maxValue;
        }
        
        private void CheckOnTwoCards(ref int value, List<Card> allCards, ref int counter, ref List<Card> winnerCards)
        {
            var index = 0;
            for (var i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 2)
                {
                    index = i;
                    break;
                }
            }
            for (var i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 2)
                {
                    if (allCards[i].Rank >= allCards[index].Rank)
                        index = i;
                }
            }

            if (index == 0) return;

            value += (int)allCards[index].Rank * 2;
            counter++;
            winnerCards.AddRange(allCards.Where(c => c.Rank == allCards[index].Rank).Select(item => item));

            //Старая рабочая версия
            //foreach (Card currentCard in temp)
            //{
            //    if (temp.FindAll(Card => Card.value == currentCard.value).Count >= 2)
            //    {
            //        Value += (int)currentCard.value * 2;
            //        counter++;
            //        newTemp.AddRange(temp.Where(g => g.value == currentCard.value).Select(item => item));
            //        break;
            //    }
            //}
        }

        private void CheckOnThreeCard(ref int value, List<Card> allCards, ref int lastValue, ref int counter, ref List<Card> winnerCards)
        {
            var index = 0;
            for (var i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 3)
                {
                    index = i;
                    break;
                }
            }
            for (int i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count == 3)
                {
                    if (allCards[i].Rank >= allCards[index].Rank)
                        index = i;                
                }
            }

            if (index == 0) return;

            value += (int)allCards[index].Rank * 3;
            lastValue = (int)allCards[index].Rank;
            counter++;
            winnerCards.AddRange(allCards.Where(c => c.Rank == allCards[index].Rank).Select(card => card));
        }
    }
}