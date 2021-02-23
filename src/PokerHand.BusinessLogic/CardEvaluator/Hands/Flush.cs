using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class Flush : IRules
    {
        private const int Rate = 1300;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var result = new EvaluationResult();
            var numbersOfSuits = new Dictionary<int, int>();;
            
            var allCards = tableCards.Concat(playerHand).ToList();
            
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);
            
            if (numberOfJokers > 0)
            {
                foreach (var card in allCards.Where(c => c.Rank is not CardRankType.Joker))
                {
                    if (!numbersOfSuits.ContainsKey((int) card.Suit))
                        numbersOfSuits.Add((int) card.Suit, 1);
                    else
                        numbersOfSuits[(int) card.Suit]++;
                }

                if (numbersOfSuits.Any(c => c.Value >= (5 - numberOfJokers)) is false)
                    return result;

                result.IsWinningHand = true;
                result.EvaluatedHand.HandType = HandType.Flush;
                result.EvaluatedHand.Cards = new List<Card>();
                
                var maxSuit = (CardSuitType) numbersOfSuits
                    .Where(c => c.Value >= (5 - numberOfJokers))
                    .OrderByDescending(c => c.Key)
                    .First()
                    .Key;

                var cardsToAdd = allCards
                    .Where(c => c.Suit == maxSuit)
                    .OrderByDescending(c => c.Rank)
                    .ToList();

                for (var index = 0; index < 5 - numberOfJokers; index++)
                {
                    result.EvaluatedHand.Cards.Add(cardsToAdd[index]);
                    result.EvaluatedHand.Value += (int) cardsToAdd[index].Rank * Rate;
                }
                
                // Add jokers
                foreach (var card in allCards.Where(c => c.Rank is CardRankType.Joker))
                {
                    var maxValue = GetMaxValue(allCards.Where(c => c.Rank is not CardRankType.Joker).ToList());
                    
                    result.EvaluatedHand.Cards.Add(card);
                    result.EvaluatedHand.Value += maxValue * Rate;
                }

                return result;
            }

            foreach (var card in allCards)
            {
                if (!numbersOfSuits.ContainsKey((int) card.Suit))
                    numbersOfSuits.Add((int) card.Suit, 1);
                else
                    numbersOfSuits[(int) card.Suit]++;
            }

            if (numbersOfSuits.Any(c => c.Value >= 5) is false)
                return result;

            result.IsWinningHand = true;
            result.EvaluatedHand.HandType = HandType.Flush;
            result.EvaluatedHand.Cards = new List<Card>();
                
            var winningSuit = (CardSuitType) numbersOfSuits
                .First(c => c.Value >= 5)
                .Key;

            var winningCards = allCards
                .Where(c => c.Suit == winningSuit)
                .OrderByDescending(c => c.Rank)
                .ToList();

            for (var index = 0; index < 5; index++)
            {
                result.EvaluatedHand.Cards.Add(winningCards[index]);
                result.EvaluatedHand.Value += (int) winningCards[index].Rank * Rate;
            }

            result.EvaluatedHand.Cards = result.EvaluatedHand.Cards.OrderByDescending(c => c.Rank).Reverse().ToList();

            return result;
        }

        private int GetMaxValue(List<Card> cards)
        {
            var maxValue = 0;
        
            foreach (var card in cards)
            {
                if (maxValue < (int)card.Rank)
                {
                    if (card.Rank is not CardRankType.Joker)
                        maxValue = (int)card.Rank;
                }
            }
        
            return maxValue;
        }
    }
}
