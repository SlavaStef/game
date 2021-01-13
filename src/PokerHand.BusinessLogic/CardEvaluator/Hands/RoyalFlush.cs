using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class RoyalFlush : IRules
    {
        private const int Rate = 200000;
        
        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            var isRoyalFlush = false;
            value = 0;
            finalCardsList = new List<Card>();

            CheckCombination(isJokerGame, ref value, playerHand, tableCards, ref isRoyalFlush);
            value = isRoyalFlush 
                ? value *= Rate 
                : 0;

            if (isRoyalFlush)
            {
                value *= Rate;
                handType = HandType.RoyalFlush;
            }
            else
            {
                value = 0;
                handType = HandType.None;
            }
            
            return isRoyalFlush;
        }

        private void CheckCombination(bool isJokerGame, ref int value, List<Card> playerHand, List<Card> tableCards, ref bool isRoyalFlush)
        {
            var allCards = tableCards.Concat(playerHand).ToList();
            
            if (new StraightFlush().Check(playerHand, tableCards, isJokerGame, out int outValue, out HandType handType, out List<Card> newCards))
            {
                SortByRank(newCards);
                newCards.Reverse();
                CheckJokerCount(isJokerGame, allCards, newCards);

                if (newCards.Count > 4)
                {
                    if (newCards[0].Rank == CardRankType.Ace
                        && newCards[1].Rank == CardRankType.King
                        && newCards[2].Rank == CardRankType.Queen
                        && newCards[3].Rank == CardRankType.Jack
                        && newCards[4].Rank == CardRankType.Ten)
                    {
                        foreach (var card in allCards)
                            value += (int)card.Rank;

                        isRoyalFlush = true;
                    }
                }
            }
        }
        
        private void SortByRank(List<Card> cards)
        {
            var tempCard = new Card();
            
            for (var i = 0; i < cards.Count - 1; i++)
            {
                for (var j = i + 1; j < cards.Count; j++)
                {
                    if (cards[i].Rank > cards[j].Rank)
                    {
                        tempCard = cards[i];
                        cards[i] = cards[j];
                        cards[j] = tempCard;
                    }
                }
            }
        }

        private void CheckJokerCount(bool isJokerGame, List<Card> allCards, List<Card> newCards)
        {
            if (!isJokerGame) return;

            var trueCounter = 0;
            var jokerCount = 0;
            jokerCount += (allCards.Where(card => card.Rank == CardRankType.Joker).Select(card => card)).Count();

            if (newCards[0].Rank == CardRankType.Ace) trueCounter++;
            if (newCards[1].Rank == CardRankType.King) trueCounter++;
            if (newCards[2].Rank == CardRankType.Queen) trueCounter++;
            if (newCards[3].Rank == CardRankType.Jack) trueCounter++;
            if (newCards[4].Rank == CardRankType.Ten) trueCounter++;

            if (jokerCount == 1)
                OneJokerCheck(trueCounter, newCards);
            else
                TwoJokerCheck(trueCounter, newCards);
        }
        
        private void OneJokerCheck(int trueCounter, List<Card> newCards)
        {
            if (trueCounter == 4)
            {
                foreach (var card in newCards)
                {
                    if (card.Rank == CardRankType.Joker)
                    {
                        if (newCards[0].Rank != CardRankType.Ace) { card.Rank = CardRankType.Ace; break; }
                        if (newCards[1].Rank != CardRankType.King) { card.Rank = CardRankType.King; break; }
                        if (newCards[2].Rank != CardRankType.Queen) { card.Rank = CardRankType.Queen; break; }
                        if (newCards[3].Rank != CardRankType.Jack) { card.Rank = CardRankType.Jack; break; }
                        if (newCards[4].Rank != CardRankType.Ten) { card.Rank = CardRankType.Ten; break; }
                    }
                }
            }
        }

        private void TwoJokerCheck(int trueCounter, List<Card> newCards)
        {
            if (trueCounter != 3) return;
            
            foreach (var card in newCards)
            {
                if (card.Rank == CardRankType.Joker)
                {
                    if (newCards[0].Rank != CardRankType.Ace) { card.Rank = CardRankType.Ace; break; }
                    if (newCards[1].Rank != CardRankType.King) { card.Rank = CardRankType.King; break; }
                    if (newCards[2].Rank != CardRankType.Queen) { card.Rank = CardRankType.Queen; break; }
                    if (newCards[3].Rank != CardRankType.Jack) { card.Rank = CardRankType.Jack; break; }
                    if (newCards[4].Rank != CardRankType.Ten) { card.Rank = CardRankType.Ten; break; }
                }
            }
        }
    }
}
