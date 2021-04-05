using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic
{
    public class ThreeOfAKind : IRules
    {
        private const int Rate = 170;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            switch (numberOfJokers)
            {
                case 0:
                    foreach (var card in allCards)
                    {
                        var possibleThreeOfAKind = allCards
                            .Where(c => c.Rank == card.Rank)
                            .ToList();

                        if (possibleThreeOfAKind.Count is not 3) 
                            continue;
                        
                        result.IsWinningHand = true;
                        result.Hand.HandType = HandType.ThreeOfAKind;
                        result.Hand.Value = (int) card.Rank * 3 * Rate;
                        result.Hand.Cards = possibleThreeOfAKind.ToList();

                        foreach (var cardToRemove in possibleThreeOfAKind) 
                            allCards.Remove(cardToRemove);
                    
                        AddSideCards(result.Hand.Cards, allCards);
                        
                        for (var index = 3; index < 5; index++)
                            result.Hand.Value += (int)result.Hand.Cards[index].Rank;

                        return result;
                    }
                    
                    break;
                case 1:
                    var joker = allCards
                        .First(c => c.Rank is CardRankType.Joker);
                    
                    foreach (var card in allCards)
                    {
                        var possibleThreeOfAKind = allCards
                            .Where(c => c.Rank is not CardRankType.Joker && c.Rank == card.Rank)
                            .ToList();

                        if (possibleThreeOfAKind.Count < 2) 
                            continue;
                        
                        result.IsWinningHand = true;
                        result.Hand.HandType = HandType.ThreeOfAKind;
                        result.Hand.Value = (int) card.Rank * 3 * Rate;

                        result.Hand.Cards = new List<Card>(5);
                        result.Hand.Cards.Add(possibleThreeOfAKind[0]);
                        result.Hand.Cards.Add(possibleThreeOfAKind[1]);

                        joker.SubstitutedCard = new Card();
                        joker.SubstitutedCard.Rank = card.Rank;
                        result.Hand.Cards.Add(joker);
                        
                        for (var index = 0; index < 2; index++)
                            allCards.Remove(possibleThreeOfAKind[index]);

                        AddSideCards(result.Hand.Cards, allCards);
                        
                        for (var index = 3; index < 5; index++)
                            result.Hand.Value += (int)result.Hand.Cards[index].Rank;

                        return result;
                    }
                    
                    break;
                case 2:
                    result.IsWinningHand = true;
                    result.Hand.HandType = HandType.ThreeOfAKind;
                    result.Hand.Cards = new List<Card>(5);

                    var maxRankCard = allCards
                        .Where(c => c.Rank is not CardRankType.Joker)
                        .OrderByDescending(c => c.Rank)
                        .First();
                    result.Hand.Cards.Add(maxRankCard);
                    
                    var jokers = allCards
                        .Where(c => c.Rank is CardRankType.Joker)
                        .ToList();

                    foreach (var card in jokers)
                    {
                        card.SubstitutedCard = new Card();
                        card.SubstitutedCard.Rank = maxRankCard.Rank;
                    }
                    
                    result.Hand.Cards.AddRange(jokers);

                    result.Hand.Value = (int) maxRankCard.Rank * 3 * Rate;

                    allCards.RemoveAll(c => c.Rank is CardRankType.Joker);
                    allCards.Remove(maxRankCard);
                    AddSideCards(result.Hand.Cards, allCards);

                    for (var index = 3; index < 5; index++)
                        result.Hand.Value += (int)result.Hand.Cards[index].Rank;

                    return result;
            }

            return result;
        }
        
        private static void AddSideCards(List<Card> finalCardsList, List<Card> allCards)
        {
            allCards = allCards
                .Where(c => c.Rank is not CardRankType.Joker)
                .OrderByDescending(c => c.Rank)
                .ToList();
            
            for (var index = 0; index < 2; index++)
                finalCardsList.Add(allCards[index]);
        }
    }
}
