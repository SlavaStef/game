using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class TwoPairs : IRules
    {
        private const int Rate = 17;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult {EvaluatedHand = {Cards = new List<Card>(5)}};

            var allCards = tableCards.Concat(playerHand).ToList();
            
            // Substitute Joker with a card of max value 
            if (allCards.Any(c => c.Rank is CardRankType.Joker))
            {
                foreach (var card in allCards.Where(card => card.Rank == CardRankType.Joker))
                {
                    card.Rank = (CardRankType)GetMaxCardValue(allCards);
                    card.WasJoker = true;
                }
            }
            
            var numberOfPairs = 0;

            for (var counter = 0; counter < 2; counter++)
            {
                foreach (var card in allCards)
                {
                    if (allCards.Count(c => c.Rank == card.Rank) is 2)
                    {
                        numberOfPairs++;
                        
                        result.EvaluatedHand.Value += (int)card.Rank * 2 * Rate;
                        
                        var cardsToAdd = allCards
                            .Where(c => c.Rank == card.Rank)
                            .ToArray();

                        if (allCards.Any(c => c.Rank is CardRankType.Joker))
                        {
                            foreach (var cardToAdd in cardsToAdd.Where(cardToAdd => cardToAdd.WasJoker))
                                cardToAdd.Rank = CardRankType.Joker;
                        }
                        
                        result.EvaluatedHand.Cards.AddRange(cardsToAdd);
                        
                        allCards.RemoveAll(c => cardsToAdd.Contains(c));
                        break;
                    }
                }
                
                result.EvaluatedHand.Cards = result.EvaluatedHand.Cards
                    .OrderByDescending(c => c.Rank)
                    .ToList();
                
                if (numberOfPairs is 0)
                    break;
            }

            if (numberOfPairs is 2)
            {
                result.IsWinningHand = true;
                result.EvaluatedHand.HandType = HandType.TwoPairs;

                AddSideCards(result.EvaluatedHand.Cards, allCards);
            }
            else
            {
                result.EvaluatedHand.Value = 0;
                result.EvaluatedHand.Cards = null;
            }

            return result;
        }
        
        private void AddSideCards(List<Card> finalCardsList, List<Card> allCards)
        {
            allCards = allCards
                .OrderByDescending(c => c.Rank)
                .ToList();
            
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