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

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            
            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            switch (numberOfJokers)
            {
                case 0:
                    foreach (var card in allCards.Where(card => allCards.Count(c => c.Rank == card.Rank) is 2))
                    {
                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.OnePair;
                        
                        var cardsToAdd = allCards
                            .Where(c => c.Rank == card.Rank)
                            .ToList();
                        
                        result.EvaluatedHand.Cards = new List<Card>(5);
                        result.EvaluatedHand.Cards.AddRange(cardsToAdd);

                        foreach (var c in cardsToAdd)
                            allCards.Remove(c);
                        
                        result.EvaluatedHand.Cards.AddRange(GetSideCards(allCards));
                        
                        result.EvaluatedHand.Value = CalculateHandValue(result.EvaluatedHand.Cards);
                        return result;
                    }

                    return result;
                case 1:
                    result.IsWinningHand = true;
                    result.EvaluatedHand.HandType = HandType.OnePair;

                    var highestCard = allCards
                        .Where(c => c.Rank is not CardRankType.Joker)
                        .OrderByDescending(c => c.Rank)
                        .First();
                    
                    result.EvaluatedHand.Cards = new List<Card>(5);
                    result.EvaluatedHand.Cards.Add(highestCard);
                    allCards.Remove(highestCard);
                    
                    result.EvaluatedHand.Cards.Add(allCards.First(c => c.Rank is CardRankType.Joker));
                    allCards.Remove(allCards.First(c => c.Rank is CardRankType.Joker));
                    
                    result.EvaluatedHand.Cards.AddRange(GetSideCards(allCards));
                    
                    result.EvaluatedHand.Value = CalculateHandValue(result.EvaluatedHand.Cards);
                    return result;
            }

            return result;
        }

        private static int CalculateHandValue(List<Card> cards)
        {
            var value = 0;

            value += (int)cards[0].Rank * 2 * Rate;

            for (var index = 2; index < 5; index++)
                value += (int) cards[index].Rank;

            return value;
        }

        private static IEnumerable<Card> GetSideCards(List<Card> cards)
        {
            var sideCards = new List<Card>(3);
            
            cards = cards
                .OrderByDescending(c => c.Rank)
                .ToList();

            for (var i = 0; i < 3; i++)
                sideCards.Add(cards[i]);

            return sideCards;
        }
    }
}