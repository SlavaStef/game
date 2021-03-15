using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Hands
{
    public class StraightFlush : IRules
    {
        private const int Rate = 180000;
        
        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            switch (numberOfJokers)
            {
                case 0:
                    return CheckWithNoJokers(playerHand, tableCards, result);
                case 1:
                    return CheckWithOneJoker(playerHand, tableCards, result);
                case 2:
                    return result;
                default:
                    return result;
            }
            
            

            // var firstCardSuit = isStraightResult.EvaluatedHand.Cards[0].Suit;
            // var isStraightFlush = isStraightResult.EvaluatedHand.Cards.Where(c => c.Rank is not CardRankType.Joker)
            //     .All(c => c.Suit == firstCardSuit);
            
            // switch (allCards.Count(c => c.Rank is CardRankType.Joker))
            // {
            //     case 0:
            //         if (isStraightFlush is false)
            //             return result;
            //
            //         result.IsWinningHand = true;
            //         result.EvaluatedHand.HandType = HandType.StraightFlush;
            //
            //         foreach (var card in isStraightResult.EvaluatedHand.Cards)
            //             result.EvaluatedHand.Value += (int) card.Rank * Rate;
            //
            //         result.EvaluatedHand.Cards = isStraightResult.EvaluatedHand.Cards.ToList();
            //         break;
            //     case 1:
            //         if (isStraightFlush is false)
            //         {
            //             if (isStraightResult.EvaluatedHand.Cards.Where(c => c.Rank is not CardRankType.Joker).Count(c => c.Suit != firstCardSuit) is not 1)
            //                 return result;
            //
            //             foreach (var card in isStraightResult.EvaluatedHand.Cards)
            //                 result.EvaluatedHand.Value += (int) card.Rank * Rate;
            //             
            //             var cardToRemove = isStraightResult.EvaluatedHand.Cards.First(c => c.Suit != firstCardSuit);
            //             isStraightResult.EvaluatedHand.Cards.Remove(cardToRemove);
            //             isStraightResult.EvaluatedHand.Cards.Add(allCards.First(c => c.Rank is CardRankType.Joker));
            //             
            //             result.IsWinningHand = true;
            //             result.EvaluatedHand.HandType = HandType.StraightFlush;
            //             result.EvaluatedHand.Cards = isStraightResult.EvaluatedHand.Cards.ToList();
            //             return result;
            //         }
            //         
            //         result.IsWinningHand = true;
            //         result.EvaluatedHand.HandType = HandType.StraightFlush;
            //
            //         foreach (var card in isStraightResult.EvaluatedHand.Cards)
            //         {
            //             if (card.Rank is CardRankType.Joker)
            //                 result.EvaluatedHand.Value += ((int)isStraightResult.EvaluatedHand.Cards
            //                     .Where(c => c.Rank is not CardRankType.Joker).OrderByDescending(c => c.Rank).First()
            //                     .Rank + 1) * Rate;
            //             else
            //                 result.EvaluatedHand.Value += (int) card.Rank * Rate;
            //         }
            //         
            //         result.EvaluatedHand.Cards = isStraightResult.EvaluatedHand.Cards.OrderByDescending(c => c.Rank).ToList();
            //         return result;
            //     case 2:
            //         if (isStraightFlush is false)
            //         {
            //             if (isStraightResult.EvaluatedHand.Cards.Where(c => c.Rank is not CardRankType.Joker).Count(c => c.Suit != firstCardSuit) is not 1 or 2)
            //                 return result;
            //
            //             foreach (var card in isStraightResult.EvaluatedHand.Cards)
            //                 result.EvaluatedHand.Value += (int) card.Rank * Rate;
            //             
            //             var cardsToRemove = isStraightResult.EvaluatedHand.Cards.Where(c => c.Suit != firstCardSuit).ToList();
            //
            //             foreach (var card in cardsToRemove)
            //                 isStraightResult.EvaluatedHand.Cards.Remove(card);
            //
            //             if (cardsToRemove.Count is 1) 
            //                 isStraightResult.EvaluatedHand.Cards.Add(allCards.First(c => c.Rank is CardRankType.Joker));
            //             else
            //                 isStraightResult.EvaluatedHand.Cards.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker));
            //             
            //             result.IsWinningHand = true;
            //             result.EvaluatedHand.HandType = HandType.StraightFlush;
            //             result.EvaluatedHand.Cards = isStraightResult.EvaluatedHand.Cards.ToList();
            //             return result;
            //         }
            //         
            //         result.IsWinningHand = true;
            //         result.EvaluatedHand.HandType = HandType.StraightFlush;
            //
            //         foreach (var card in isStraightResult.EvaluatedHand.Cards)
            //             result.EvaluatedHand.Value += (int) card.Rank * Rate;
            //
            //         result.EvaluatedHand.Cards = isStraightResult.EvaluatedHand.Cards.OrderByDescending(c => c.Rank).ToList();
            //         return result;
            // }

        }

        private EvaluationResult CheckWithOneJoker(List<Card> playerHand, List<Card> tableCards, EvaluationResult result)
        {
            var isStraightResult = new Straight().Check(playerHand, tableCards);
            if (isStraightResult.IsWinningHand is false)
                return result;

            var (isStraightFlush, numberOfUsedJokers) =
                AnalyzeStraightCardsSuits(isStraightResult.Hand.Cards);

            if (isStraightFlush is false)
                return result;

            result.IsWinningHand = true;
            result.Hand.HandType = HandType.StraightFlush;
            result.Hand.Cards = isStraightResult.Hand.Cards.ToList();

            var joker = isStraightResult.Hand.Cards.First(c => c.Rank is CardRankType.Joker);
            joker.SubstitutedCard.Suit = result.Hand.Cards[0].Suit;

            EvaluateHand(result);

            result.Hand.Cards = SortByDescending(result.Hand.Cards);

            return result;
        }

        private List<Card> SortByDescending(List<Card> cards)
        {
            foreach (var card in cards.Where(c => c.Rank is CardRankType.Joker))
            {
                card.Rank = card.SubstitutedCard.Rank;
            }

            cards = cards
                .OrderByDescending(c => c.Rank)
                .ToList();

            foreach (var card in cards.Where(c => c.SubstitutedCard is not null))
            {
                card.Rank = CardRankType.Joker;
            }

            return cards;
        }

        private static void EvaluateHand(EvaluationResult result)
        {
            foreach (var card in result.Hand.Cards)
            {
                if (card.Rank is CardRankType.Joker)
                {
                   result.Hand.Value += (int) card.SubstitutedCard.Rank * Rate;
                   continue;
                }
                
                result.Hand.Value += (int) card.Rank * Rate;
            }
                
        }

        private EvaluationResult CheckWithNoJokers(List<Card> playerHand, List<Card> tableCards, EvaluationResult result)
        {
            var isStraightResult = new Straight().Check(playerHand, tableCards);
            if (isStraightResult.IsWinningHand is false)
                return result;

            var (isStraightFlush, numberOfUsedJokers) =
                AnalyzeStraightCardsSuits(isStraightResult.Hand.Cards);

            if (isStraightFlush is false)
                return result;

            result.IsWinningHand = true;
            result.Hand.HandType = HandType.StraightFlush;
            result.Hand.Cards = isStraightResult.Hand.Cards.ToList();

            foreach (var card in result.Hand.Cards)
                result.Hand.Value += (int) card.Rank * Rate;

            return result;
        }

        private (bool, int) AnalyzeStraightCardsSuits(List<Card> cards)
        {
            var cardsWithoutJokers = cards.Where(c => c.Rank is not CardRankType.Joker).ToList();
            if (cardsWithoutJokers.All(c => c.Suit == cardsWithoutJokers[0].Suit))
                return (true, cards.Count - cardsWithoutJokers.Count);

            return (false, cards.Count - cardsWithoutJokers.Count);
        }
    }
}
