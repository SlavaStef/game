using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Hands
{
    public class FullHouse : IRules
    {
        private const int Rate = 6700;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult {Hand = {Cards = new List<Card>()}};

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
            checkResultZeroJokers.Hand.HandType = HandType.FullHouse;
            checkResultZeroJokers.Hand.Cards = threeCards.Concat(onePair).ToList();
            checkResultZeroJokers.Hand.Value =
                ((int) threeCards[0].Rank * 3 + (int) onePair[0].Rank * 2) * Rate;

            return checkResultZeroJokers;
        }
        
        private EvaluationResult CheckWithOneJoker(List<Card> allCards)
        {
            var checkResultOneJoker = new EvaluationResult();
            Card joker;

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
                joker.SubstitutedCard = new Card {Rank = (CardRankType) highestRankFromTwoPairs};

                checkResultOneJoker.IsWinningHand = true;
                checkResultOneJoker.Hand.HandType = HandType.FullHouse;

                checkResultOneJoker.Hand.Cards = new List<Card>(5) {joker};
                checkResultOneJoker.Hand.Cards.AddRange(
                    twoPairsCheckResult.twoPairs.OrderByDescending(c => c.Rank));
                checkResultOneJoker.Hand.Value = ((int) twoPairsCheckResult.twoPairs[0].Rank * 2 +
                                                           (int) twoPairsCheckResult.twoPairs[2].Rank * 2 +
                                                           highestRankFromTwoPairs) * Rate;

                return checkResultOneJoker;
            }

            var maxCard = newCards
                .First(c => (int) c.Rank == newCards.Where(card => card.Rank != CardRankType.Joker)
                    .Select(card => (int) card.Rank).Max());

            joker = newCards.First(c => c.Rank == CardRankType.Joker);
            joker.SubstitutedCard = new Card {Rank = maxCard.Rank};

            checkResultOneJoker.IsWinningHand = true;
            checkResultOneJoker.Hand.HandType = HandType.FullHouse;
            checkResultOneJoker.Hand.Cards =
                threeCard.Concat(new List<Card> {maxCard, joker}).ToList();
            checkResultOneJoker.Hand.Value =
                ((int) threeCard[0].Rank * 3 + (int) maxCard.Rank * 2) * Rate;

            return checkResultOneJoker;
        }
        
        private EvaluationResult CheckWithTwoJokers(List<Card> allCards)
        {
            var checkResultTwoJokers = new EvaluationResult();

            // ThreeOfAKind + (Joker + Joker)
            // TODO: situation: (one card from threeOfAKind + Joker + Joker) + highCard + highCard
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

                foreach (var card in jokersFromList)
                {
                    card.SubstitutedCard = new Card {Rank = maxCardFromPair.Rank};
                    card.Rank = maxCardFromPair.Rank;
                }
                
                checkResultTwoJokers.IsWinningHand = true;
                checkResultTwoJokers.Hand.HandType = HandType.FullHouse;
                checkResultTwoJokers.Hand.Cards =
                    onePair.Concat(jokersFromList).Concat(new List<Card> {maxCardFromPair}).ToList();
                checkResultTwoJokers.Hand.Value =
                    ((int) onePair[0].Rank * 2 + (int) maxCardFromPair.Rank * 3) * Rate;

                checkResultTwoJokers.Hand.Cards =
                    checkResultTwoJokers.Hand.Cards.OrderByDescending(c => c.Rank).ToList();

                foreach (var card in checkResultTwoJokers.Hand.Cards.Where(card => card.SubstitutedCard is not null))
                    card.Rank = CardRankType.Joker;

                return checkResultTwoJokers;
            }

            var maxRank =
                GetMaxAvailableRank(allCards.Except(threeCard).Where(c => c.Rank is not CardRankType.Joker).ToList(),
                    threeCard[0].Rank);

            var jokers = newCards.Where(c => c.Rank == CardRankType.Joker).ToList();

            foreach (var card in jokers)
                card.SubstitutedCard = new Card {Rank = (CardRankType) maxRank};

            checkResultTwoJokers.IsWinningHand = true;
            checkResultTwoJokers.Hand.HandType = HandType.FullHouse;
            
            checkResultTwoJokers.Hand.Cards = threeCard.Concat(jokers).ToList();
            checkResultTwoJokers.Hand.Value =
                ((int) threeCard[0].Rank * 3 + maxRank * 2) * Rate;

            return checkResultTwoJokers;
        }

        private int GetMaxAvailableRank(List<Card> cards, CardRankType rank)
        {
            for (var cardRank = CardRankType.Ace; cardRank >= CardRankType.Deuce; cardRank--)
            {
                if (cardRank != rank && cards.Any(c => c.Rank == cardRank) is false)
                    return (int)cardRank;
            }

            return 2;
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