using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class Straight : IRules
    {
        private const int Rate = 400;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var result = new EvaluationResult();
            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);
            var jokers = allCards.Where(c => c.Rank is CardRankType.Joker).ToList();


            allCards = allCards
                .Where(c => c.Rank is not CardRankType.Joker)
                .GroupBy(c => c.Rank)
                .Select(g => g.First())
                .OrderByDescending(c => c.Rank)
                .ToList();
            allCards.AddRange(jokers);

            if (allCards.Count < 5)
                return result;

            switch (numberOfJokers)
            {
                case 0:
                    for (var index = 0; index < allCards.Count - 4; index++)
                    {

                        var isStraight =    (int) allCards[index].Rank == (int) allCards[index + 1].Rank + 1
                                         && (int) allCards[index].Rank == (int) allCards[index + 2].Rank + 2
                                         && (int) allCards[index].Rank == (int) allCards[index + 3].Rank + 3
                                         && (int) allCards[index].Rank == (int) allCards[index + 4].Rank + 4;

                        if (isStraight is false)
                            continue;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();
                        for (var i = index; i <= index + 4; i++)
                        {
                            cardsToAdd.Add(allCards[i]);
                            result.EvaluatedHand.Value += (int) allCards[i].Rank * Rate;
                        }


                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    // Check on low ace
                    if (allCards.Any(c => c.Rank is CardRankType.Ace))
                    {
                        var isStraight = allCards.Any(c => c.Rank is CardRankType.Deuce) &&
                                         allCards.Any(c => c.Rank is CardRankType.Three) &&
                                         allCards.Any(c => c.Rank is CardRankType.Four) &&
                                         allCards.Any(c => c.Rank is CardRankType.Five);

                        if (isStraight is false)
                            return result;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();

                        for (var i = 0; i < 4; i++)
                        {
                            cardsToAdd.Add(allCards.First(c => (int) c.Rank == i + 2));
                            result.EvaluatedHand.Value += (int) cardsToAdd.Last().Rank * Rate;
                        }

                        cardsToAdd.Add(allCards.First(c => c.Rank is CardRankType.Ace));
                        result.EvaluatedHand.Value += (int) cardsToAdd.Last().Rank * Rate;

                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    return result;
                case 1:
                    // Check for Five straight cards
                    for (var index = 0; index < allCards.Count - 4; index++)
                    {
                        if (allCards[index].Rank is CardRankType.Joker)
                            continue;

                        var isFiveStraight = (int) allCards[index].Rank == (int) allCards[index + 1].Rank + 1
                                             && (int) allCards[index].Rank == (int) allCards[index + 2].Rank + 2
                                             && (int) allCards[index].Rank == (int) allCards[index + 3].Rank + 3
                                             && (int) allCards[index].Rank == (int) allCards[index + 4].Rank + 4;

                        if (isFiveStraight is false)
                            continue;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();
                        for (var i = index; i <= index + 4; i++)
                            cardsToAdd.Add(allCards[i]);

                        // Add Joker
                        var maxCardRank = cardsToAdd[0].Rank;
                        if (maxCardRank is not CardRankType.Ace)
                        {
                            cardsToAdd.Remove(cardsToAdd.Last());
                            cardsToAdd.Add(allCards.First(c => c.Rank is CardRankType.Joker));

                            result.EvaluatedHand.Value += ((int) allCards[index].Rank * 5 - 5) * Rate;
                        }
                        else
                            result.EvaluatedHand.Value += 60 * Rate;

                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    // Check for Four straight cards
                    for (var index = 0; index < allCards.Count - 3; index++)
                    {
                        if (allCards[index].Rank is CardRankType.Joker)
                            continue;

                        var isFourStraight = (int) allCards[index].Rank == (int) allCards[index + 1].Rank + 1
                                             && (int) allCards[index].Rank == (int) allCards[index + 2].Rank + 2
                                             && (int) allCards[index].Rank == (int) allCards[index + 3].Rank + 3;

                        if (isFourStraight is false)
                            continue;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();
                        for (var i = index; i <= index + 3; i++)
                        {
                            cardsToAdd.Add(allCards[i]);
                            result.EvaluatedHand.Value += (int) allCards[i].Rank * Rate;
                        }

                        // Add Joker
                        var maxCardRank = cardsToAdd.Where(c => c.Rank is not CardRankType.Joker)
                            .OrderByDescending(c => c.Rank).First().Rank;
                        if (maxCardRank is CardRankType.Ace)
                            result.EvaluatedHand.Value += (int) CardRankType.Ten * Rate;
                        else
                            result.EvaluatedHand.Value += ((int) maxCardRank + 1) * Rate;

                        cardsToAdd = cardsToAdd.OrderBy(c => c.Rank).ToList();
                        cardsToAdd.Add(allCards.First(c => c.Rank is CardRankType.Joker));
                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    // Check for Three straight and One card AND for 2 x Two straight
                    for (var index = 0; index < allCards.Count - 3; index++)
                    {
                        if (allCards[index].Rank is CardRankType.Joker)
                            continue;

                        if ((int) allCards[index].Rank != (int) allCards[index + 3].Rank + 4)
                            continue;

                        result.IsWinningHand = true;

                        result.EvaluatedHand.HandType = HandType.Straight;
                        result.EvaluatedHand.Cards = new List<Card>();

                        for (var i = 0; i < 4; i++)
                            result.EvaluatedHand.Cards.Add(allCards[index + i]);

                        result.EvaluatedHand.Cards.Add(allCards.First(c => c.Rank is CardRankType.Joker));

                        result.EvaluatedHand.Value = ((int) allCards[index].Rank * 5 - 10) * Rate;

                        return result;
                    }

                    break;
                case 2:
                    // Check for Five straight cards
                    for (var index = 0; index < allCards.Count - 4; index++)
                    {
                        if (allCards[index].Rank is CardRankType.Joker)
                            continue;

                        var isFiveStraight =    (int) allCards[index].Rank == (int) allCards[index + 1].Rank + 1
                                             && (int) allCards[index].Rank == (int) allCards[index + 2].Rank + 2
                                             && (int) allCards[index].Rank == (int) allCards[index + 3].Rank + 3
                                             && (int) allCards[index].Rank == (int) allCards[index + 4].Rank + 4;

                        if (isFiveStraight is false)
                            continue;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();
                        for (var i = index; i <= index + 4; i++)
                            cardsToAdd.Add(allCards[i]);

                        // Add Jokers
                        var maxCardRank = cardsToAdd
                            .Where(c => c.Rank is not CardRankType.Joker)
                            .OrderByDescending(c => c.Rank)
                            .First()
                            .Rank;

                        switch (maxCardRank)
                        {
                            case CardRankType.King:
                                cardsToAdd.Remove(cardsToAdd.Last());

                                foreach (var card in cardsToAdd)
                                    result.EvaluatedHand.Value += (int) card.Rank * Rate;

                                result.EvaluatedHand.Value += (int) CardRankType.Ace * Rate;

                                cardsToAdd.Add(allCards.First(c => c.Rank is CardRankType.Joker));
                                break;
                            case CardRankType.Ace:
                                foreach (var card in cardsToAdd)
                                    result.EvaluatedHand.Value += (int) card.Rank * Rate;
                                break;
                            default:
                                cardsToAdd.Remove(cardsToAdd.Last());
                                cardsToAdd.Remove(cardsToAdd.Last());

                                foreach (var card in cardsToAdd)
                                    result.EvaluatedHand.Value += (int) card.Rank * Rate;

                                result.EvaluatedHand.Value += ((int) maxCardRank * 2 + 3) * Rate;

                                cardsToAdd.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker));
                                break;
                        }

                        cardsToAdd = cardsToAdd.OrderByDescending(c => c.Rank).ToList();
                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    // Check fot Four straight cards
                    for (var index = 0; index < allCards.Count - 3; index++)
                    {
                        if (allCards[index].Rank is CardRankType.Joker)
                            continue;

                        var isFourStraight = (int) allCards[index].Rank == (int) allCards[index + 1].Rank + 1
                                             && (int) allCards[index].Rank == (int) allCards[index + 2].Rank + 2
                                             && (int) allCards[index].Rank == (int) allCards[index + 3].Rank + 3;

                        if (isFourStraight is false)
                            continue;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();
                        for (var i = index; i <= index + 3; i++)
                            cardsToAdd.Add(allCards[i]);

                        // Add Jokers
                        var maxCardRank = cardsToAdd
                            .Where(c => c.Rank is not CardRankType.Joker)
                            .OrderByDescending(c => c.Rank)
                            .First()
                            .Rank;

                        switch (maxCardRank)
                        {
                            case CardRankType.King:
                                foreach (var card in cardsToAdd)
                                    result.EvaluatedHand.Value += (int) card.Rank * Rate;

                                result.EvaluatedHand.Value += (int) CardRankType.Ace * Rate;

                                cardsToAdd.Add(allCards.First(c => c.Rank is CardRankType.Joker));
                                break;
                            case CardRankType.Ace:
                                foreach (var card in cardsToAdd)
                                    result.EvaluatedHand.Value += (int) card.Rank * Rate;

                                result.EvaluatedHand.Value += (int) CardRankType.Ten * Rate;

                                cardsToAdd.Add(allCards.First(c => c.Rank is CardRankType.Joker));
                                break;
                            default:
                                cardsToAdd.Remove(cardsToAdd.Last());

                                foreach (var card in cardsToAdd)
                                    result.EvaluatedHand.Value += (int) card.Rank * Rate;

                                result.EvaluatedHand.Value += ((int) maxCardRank * 2 + 3) * Rate;

                                cardsToAdd.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker));
                                break;
                        }

                        cardsToAdd = cardsToAdd.OrderByDescending(c => c.Rank).ToList();
                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    // Check for Three straight cards
                    for (var index = 0; index < allCards.Count - 2; index++)
                    {
                        if (allCards[index].Rank is CardRankType.Joker)
                            continue;

                        var isThreeStraight = (int) allCards[index].Rank == (int) allCards[index + 1].Rank + 1
                                              && (int) allCards[index].Rank == (int) allCards[index + 2].Rank + 2;

                        if (isThreeStraight is false)
                            continue;

                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.Straight;

                        var cardsToAdd = new List<Card>();
                        for (var i = index; i <= index + 2; i++)
                        {
                            cardsToAdd.Add(allCards[i]);
                            result.EvaluatedHand.Value += (int) allCards[i].Rank * Rate;
                        }

                        // Add Jokers
                        var maxCardRank = cardsToAdd
                            .Where(c => c.Rank is not CardRankType.Joker)
                            .OrderByDescending(c => c.Rank)
                            .First()
                            .Rank;

                        switch (maxCardRank)
                        {
                            case CardRankType.King:
                                cardsToAdd.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker));
                                result.EvaluatedHand.Value += ((int) CardRankType.Ace + (int) CardRankType.Ten) * Rate;
                                break;
                            case CardRankType.Ace:
                                cardsToAdd.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker));
                                result.EvaluatedHand.Value += ((int) CardRankType.Jack + (int) CardRankType.Ten) * Rate;
                                break;
                            default:
                                cardsToAdd.AddRange(allCards.Where(c => c.Rank is CardRankType.Joker));
                                result.EvaluatedHand.Value += ((int) cardsToAdd.First().Rank * 2 + 3) * Rate;
                                break;
                        }

                        cardsToAdd = cardsToAdd.OrderByDescending(c => c.Rank).ToList();
                        result.EvaluatedHand.Cards = new List<Card>();
                        result.EvaluatedHand.Cards = cardsToAdd.ToList();

                        return result;
                    }

                    break;
            }

            return result;
        }
    }
}