using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class FiveOfAKind : IRules
    {
        private const int Rate = 300000;
        
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var result = new EvaluationResult();
            
            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            if (numberOfJokers is 0)
                return result;

            var dict = new Dictionary<CardRankType, int>();
            
            foreach (var card in allCards)
            {
                if (!dict.ContainsKey(card.Rank))
                    dict.Add(card.Rank, 1);
                else
                    dict[card.Rank]++;
            }

            var maxNumberOfSimilarRates = dict
                .Select(c => c.Value)
                .Max();

            switch (numberOfJokers)
            {
                case 1:
                    // FourOfAKind + Joker
                    if (maxNumberOfSimilarRates < 4)
                        return result;

                    result.IsWinningHand = true;
                    result.EvaluatedHand.HandType = HandType.FiveOfAKind;
                    
                    var rank = dict
                        .First(c => c.Value is 4)
                        .Key;
                    
                    var cards = allCards
                        .Where(c => c.Rank == rank)
                        .ToList();

                    result.EvaluatedHand.Cards = new List<Card>();
                    result.EvaluatedHand.Cards.AddRange(cards);
                    result.EvaluatedHand.Cards.Add(allCards.First(c => c.Rank is CardRankType.Joker));

                    result.EvaluatedHand.Value = (int) cards[0].Rank * 5 * Rate;

                    return result;
                case 2:
                    // ThreeOfAKind + Joker + Joker
                    if (maxNumberOfSimilarRates < 3)
                        return result;
                    
                    result.IsWinningHand = true;
                    result.EvaluatedHand.HandType = HandType.FiveOfAKind;
                    
                    var cardsRank = dict
                        .First(c => c.Value >= 3)
                        .Key;
                    
                    var cardsToAdd = allCards
                        .Where(c => c.Rank == cardsRank)
                        .ToList();
                    
                    result.EvaluatedHand.Cards = new List<Card>();
                    result.EvaluatedHand.Cards.AddRange(cardsToAdd);
                    result.EvaluatedHand.Cards.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker).ToList());

                    result.EvaluatedHand.Value = (int) cardsToAdd[0].Rank * 5 * Rate;

                    return result;
            }

            return result;
        }
    }
}