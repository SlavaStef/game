using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic
{
    public class FiveOfAKind : IRules
    {
        private const int Rate = 300000;
        
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
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
                    result.Hand.HandType = HandType.FiveOfAKind;
                    
                    var rank = dict
                        .First(c => c.Value is 4)
                        .Key;
                    
                    var cards = allCards
                        .Where(c => c.Rank == rank)
                        .ToList();

                    result.Hand.Cards = new List<Card>();
                    result.Hand.Cards.AddRange(cards);

                    var joker = allCards.First(c => c.Rank is CardRankType.Joker);
                    joker.SubstitutedCard = new Card {Rank = result.Hand.Cards[0].Rank};
                    result.Hand.Cards.Add(joker);

                    result.Hand.Value = (int) cards[0].Rank * 5 * Rate;

                    return result;
                case 2:
                    // ThreeOfAKind + Joker + Joker
                    if (maxNumberOfSimilarRates < 3)
                        return result;
                    
                    result.IsWinningHand = true;
                    result.Hand.HandType = HandType.FiveOfAKind;
                    
                    var cardsRank = dict
                        .First(c => c.Value >= 3)
                        .Key;
                    
                    var cardsToAdd = allCards
                        .Where(c => c.Rank == cardsRank)
                        .ToList();
                    
                    result.Hand.Cards = new List<Card>();
                    result.Hand.Cards.AddRange(cardsToAdd);
                    var jokersToAdd = allCards.Where(c => c.Rank is CardRankType.Joker).ToList();

                    foreach (var card in jokersToAdd)
                        card.SubstitutedCard = new Card {Rank = result.Hand.Cards[0].Rank};
                    
                    result.Hand.Cards.AddRange(jokersToAdd);

                    if (result.Hand.Cards.Count > 5)
                        result.Hand.Cards.Remove(result.Hand.Cards.Last());

                    result.Hand.Value = (int) cardsToAdd[0].Rank * 5 * Rate;

                    return result;
            }

            return result;
        }
    }
}