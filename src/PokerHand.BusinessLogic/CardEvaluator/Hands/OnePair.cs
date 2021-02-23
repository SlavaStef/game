using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class OnePair : IRules
    {
        private const int Rate = 5;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var result = new EvaluationResult();
            
            var allCards = tableCards.Concat(playerHand).ToList();
            
            if (isJokerGame)
            {
                foreach (var card in allCards.Where(card => card.Rank == CardRankType.Joker))
                {
                    card.Rank = (CardRankType)GetMaxCardValue(allCards);
                    card.WasJoker = true;
                }
            }

            // There can be only one pair
            foreach (var card in allCards)
            {
                if (allCards.Count(c => c.Rank == card.Rank) is 2)
                {
                    var cardsToAdd = allCards
                        .Where(c => c.Rank == card.Rank)
                        .ToList();

                    if (isJokerGame)
                    {
                        foreach (var cardToAdd in cardsToAdd.Where(cardToAdd => cardToAdd.WasJoker))
                            cardToAdd.Rank = CardRankType.Joker;
                    }

                    result.EvaluatedHand.Cards = new List<Card>(5);
                    result.EvaluatedHand.Cards.AddRange(cardsToAdd);

                    foreach (var c in cardsToAdd)
                        allCards.Remove(c);

                    AddSideCards(result.EvaluatedHand.Cards, allCards);
                    
                    result.IsWinningHand = true;
                    result.EvaluatedHand.Value = (int)card.Rank * 2 * Rate;
                    result.EvaluatedHand.HandType = HandType.OnePair;
                    
                    break;
                }
            }

            return result;
        }

        private void AddSideCards(List<Card> finalCardsList, List<Card> allCards)
        {
            allCards = allCards
                .OrderByDescending(c => c.Rank)
                .ToList();

            for (var i = 0; i < 3; i++)
                finalCardsList.Add(allCards[i]);
        }

        private int GetMaxCardValue(List<Card> cards)
        {
            var  maxValue = 0;
            
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank && card.Rank is not CardRankType.Joker)
                {
                    maxValue = (int)card.Rank;
                }
            }
            
            return maxValue;
        }
    }
}