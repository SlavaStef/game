using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.CardEvaluator.Hands
{
    public class FullHouse : IRules
    {
        private const int Rate = 6700;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
        {
            var result = new EvaluationResult {EvaluatedHand = {Cards = new List<Card>()}};

            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            switch (numberOfJokers)
            {
                case 0:
                    return CheckWithZeroJokers(allCards);
                case 1:
                    return CheckWithOneJoker(allCards);
                case 2:
                    return CheckWithTwoJokers(allCards);
            }
           
            return result;
        }

        private EvaluationResult CheckWithZeroJokers(List<Card> allCards)
        {
            var checkResultZeroJokers = new EvaluationResult();

            var (isThreeAndPair, threeCards, onePair) = CheckForThreeAndPair(allCards);

            if (isThreeAndPair is false)
            {
                checkResultZeroJokers.IsWinningHand = false;
                return checkResultZeroJokers;
            }

            checkResultZeroJokers.IsWinningHand = true;
            checkResultZeroJokers.EvaluatedHand.HandType = HandType.FullHouse;
            checkResultZeroJokers.EvaluatedHand.Cards = threeCards.Concat(onePair).ToList();
            checkResultZeroJokers.EvaluatedHand.Value =
                ((int) threeCards[0].Rank * 3 + (int) onePair[0].Rank * 2) * Rate;

            return checkResultZeroJokers;
        }
        
        private EvaluationResult CheckWithOneJoker(List<Card> allCards)
        {
            var checkResultOneJoker = new EvaluationResult();
            Card joker;
            Card maxCard;

            // ThreeOfAKind + (Joker + MaxCard)
            var (isThreeOfAKind, newCards, threeCard) = CheckForThreeOfAKind(allCards);

            if (isThreeOfAKind is false)
            {
                // TwoPairs + Joker
                var twoPairsCheckResult = CheckForTwoPairs(allCards);

                if (twoPairsCheckResult.isTwoPairs is false)
                {
                    checkResultOneJoker.IsWinningHand = false;
                    return checkResultOneJoker;
                }

                var highestRankFromTwoPairs = twoPairsCheckResult.twoPairs.Select(c => (int) c.Rank).Max();

                joker = twoPairsCheckResult.newCards.First(c => c.Rank == CardRankType.Joker);

                checkResultOneJoker.IsWinningHand = true;
                checkResultOneJoker.EvaluatedHand.HandType = HandType.FullHouse;
                checkResultOneJoker.EvaluatedHand.Cards = twoPairsCheckResult.twoPairs
                    .Concat(new List<Card>() {joker}).ToList();
                checkResultOneJoker.EvaluatedHand.Value = ((int) twoPairsCheckResult.twoPairs[0].Rank * 2 +
                                                           (int) twoPairsCheckResult.twoPairs[2].Rank * 2 +
                                                           highestRankFromTwoPairs) * Rate;

                return checkResultOneJoker;
            }

            maxCard = newCards
                .First(c => (int) c.Rank == newCards.Where(card => card.Rank != CardRankType.Joker)
                    .Select(card => (int) card.Rank).Max());

            joker = newCards.First(c => c.Rank == CardRankType.Joker);

            checkResultOneJoker.IsWinningHand = true;
            checkResultOneJoker.EvaluatedHand.HandType = HandType.FullHouse;
            checkResultOneJoker.EvaluatedHand.Cards =
                threeCard.Concat(new List<Card>() {maxCard, joker}).ToList();
            checkResultOneJoker.EvaluatedHand.Value =
                ((int) threeCard[0].Rank * 3 + (int) maxCard.Rank * 2) * Rate;

            return checkResultOneJoker;
        }
        
        private EvaluationResult CheckWithTwoJokers(List<Card> allCards)
        {
            var checkResultTwoJokers = new EvaluationResult();

            // ThreeOfAKind + (Joker + Joker)
            var (isThreeOfAKind, newCards, threeCard) = CheckForThreeOfAKind(allCards);

            if (isThreeOfAKind is false)
            {
                // OnePair + (Joker + Joker + MaxCard)

                var (isOnePair, newCardList, onePair) = CheckForOnePair(allCards);

                if (isOnePair is false)
                {
                    checkResultTwoJokers.IsWinningHand = false;
                    return checkResultTwoJokers;
                }

                var maxCardFromPair = newCardList
                    .First(c => (int) c.Rank == newCardList.Where(card => card.Rank != CardRankType.Joker)
                        .Select(card => (int) card.Rank).Max());

                var jokersFromList = allCards.Where(c => c.Rank == CardRankType.Joker).ToList();

                checkResultTwoJokers.IsWinningHand = true;
                checkResultTwoJokers.EvaluatedHand.HandType = HandType.FullHouse;
                checkResultTwoJokers.EvaluatedHand.Cards =
                    onePair.Concat(jokersFromList).Concat(new List<Card>() {maxCardFromPair}).ToList();
                checkResultTwoJokers.EvaluatedHand.Value =
                    ((int) onePair[0].Rank * 2 + (int) maxCardFromPair.Rank * 3) * Rate;

                return checkResultTwoJokers;
            }

            var maxCard = newCards
                .First(c => (int) c.Rank == newCards.Where(card => card.Rank != CardRankType.Joker)
                    .Select(card => (int) card.Rank).Max());

            var jokers = newCards.Where(c => c.Rank == CardRankType.Joker).ToList();

            checkResultTwoJokers.IsWinningHand = true;
            checkResultTwoJokers.EvaluatedHand.HandType = HandType.FullHouse;
            checkResultTwoJokers.EvaluatedHand.Cards = threeCard.Concat(jokers).ToList();
            checkResultTwoJokers.EvaluatedHand.Value =
                ((int) threeCard[0].Rank * 3 + (int) maxCard.Rank * 2) * Rate;

            return checkResultTwoJokers;
        }

        private (bool, List<Card>, List<Card>) CheckForThreeAndPair(List<Card> cards)
        {
            var (isThreeOfAKind, newCards, threeCards) = CheckForThreeOfAKind(cards);

            if (isThreeOfAKind)
            {
                var (isOnePair, newCardList, onePair) = CheckForOnePair(newCards);

                if (isOnePair)
                    return (true, threeCards, onePair);
            }

            return (false, null, null);
        }
        
        private (bool isThreeOfAKind, List<Card> newCards, List<Card> threeCards) CheckForThreeOfAKind(List<Card> allCards)
        {
            // Key is Rank, Value is number of cards
            var dict = new Dictionary<int, int>();

            foreach (var card in allCards)
            {
                if (!dict.ContainsKey((int) card.Rank))
                    dict.Add((int) card.Rank, 1);
                else
                    dict[(int) card.Rank]++;
            }

            if (dict.Any(pair => pair.Value >= 3))
            {
                var maxRank = -1;
                foreach (var (key, value) in dict.Where(p => p.Value >= 3))
                {
                    if (key >= maxRank)
                        maxRank = key;
                }

                var threeCards = allCards
                    .Where(c => (int) c.Rank == maxRank)
                    .Take(3)
                    .ToList();

                var newCards = allCards.ToList();

                foreach (var card in threeCards)
                    newCards.Remove(card);

                return (true, newCards, threeCards);
            }

            return (false, null, null);
        }
        
        private (bool isOnePair, List<Card> newCards, List<Card> onePair) CheckForOnePair(List<Card> allCards)
        {
            // Key is Rank, Value is number of cards
            var dict = new Dictionary<int, int>();

            allCards = allCards.Where(c => c.Rank is not CardRankType.Joker).ToList();

            foreach (var card in allCards)
            {
                if (!dict.ContainsKey((int) card.Rank))
                    dict.Add((int) card.Rank, 1);
                else
                    dict[(int) card.Rank]++;
            }

            if (dict.Any(pair => pair.Value >= 2))
            {
                var maxRank = -1;
                foreach (var (key, value) in dict.Where(p => p.Value >= 2))
                {
                    if (key >= maxRank)
                        maxRank = key;
                }

                var onePair = allCards
                    .Where(c => (int) c.Rank == maxRank)
                    .Take(2)
                    .ToList();

                foreach (var card in onePair)
                    allCards.Remove(card);

                return (true, allCards, onePair);
            }

            return (false, null, null);
        }

        private (bool isTwoPairs, List<Card> newCards, List<Card> twoPairs) CheckForTwoPairs(List<Card> allCards)
        {
            var twoPairs = new List<Card>();
            var numberOfPairs = 0;

            for (var counter = 0; counter < 2; counter++)
            {
                foreach (var card in allCards)
                {
                    if (allCards.Count(c => c.Rank == card.Rank) is 2)
                    {
                        numberOfPairs++;
                        
                        var cardsToAdd = allCards
                            .Where(c => c.Rank == card.Rank)
                            .ToArray();

                        twoPairs.AddRange(cardsToAdd);
                        
                        allCards.RemoveAll(c => cardsToAdd.Contains(c));
                        break;
                    }
                }
            }

            return numberOfPairs is 2
                ? (true, allCards, twoPairs)
                : (false, null, null);
        }
    }
}