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

        private void JokerCheck(bool isJokerGame, List<Card> cards)
        {
            if (!isJokerGame) return;

            var numberOfJokers = cards.Where(card => card.Rank == CardRankType.Joker).Select(card => card).Count();

            if (numberOfJokers == 1)
                OneJokerCheck(cards);
            else
                TwoJokerCheck(cards);
        }

        private void OneJokerCheck(List<Card> cards)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 3) continue;
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 2)
                {
                    foreach (var card in cards)
                        if (card.Rank == CardRankType.Joker)
                            card.Rank = cards[i].Rank;
                    break;
                }
                
                foreach (var card in cards)
                    if (card.Rank == CardRankType.Joker)
                        card.Rank = (CardRankType)GetMaxValue(cards);
            }
        }
        
        private void TwoJokerCheck(List<Card> cards)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 3) continue;
                if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 2)
                {
                    foreach (var card in cards)
                        if (card.Rank == CardRankType.Joker)
                            card.Rank = cards[i].Rank;
                    break;
                }
                foreach (var card in cards)
                    if (card.Rank == CardRankType.Joker)
                        card.Rank = (CardRankType)GetMaxValue(cards);
            }
        }
        
        private int GetMaxValue(List<Card> cards)
        {
            var maxValue = 0;
            
            for (var i = 1; i < cards.Count; i++)
            {
                if (maxValue < (int)cards[i].Rank)
                {
                    if (cards[i].Rank != CardRankType.Joker)
                        maxValue = (int)cards[i].Rank;
                }
            }
            
            return maxValue;
        }
        
        private void CheckOnTwoCards(ref int value, List<Card> allCards, ref int counter, ref List<Card> winnerCards)
        {
            var index = 0;
            for (var i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 2)
                {
                    index = i;
                    break;
                }
            }
            for (var i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 2)
                {
                    if (allCards[i].Rank >= allCards[index].Rank)
                        index = i;
                }
            }

            if (index == 0) return;

            value += (int)allCards[index].Rank * 2;
            counter++;
            winnerCards.AddRange(allCards.Where(c => c.Rank == allCards[index].Rank).Select(item => item));

            //Старая рабочая версия
            //foreach (Card currentCard in temp)
            //{
            //    if (temp.FindAll(Card => Card.value == currentCard.value).Count >= 2)
            //    {
            //        Value += (int)currentCard.value * 2;
            //        counter++;
            //        newTemp.AddRange(temp.Where(g => g.value == currentCard.value).Select(item => item));
            //        break;
            //    }
            //}
        }

        private void CheckOnThreeCard(ref int value, List<Card> allCards, ref int lastValue, ref int counter, ref List<Card> winnerCards)
        {
            var index = 0;
            for (var i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 3)
                {
                    index = i;
                    break;
                }
            }
            for (int i = 0; i < allCards.Count; i++)
            {
                if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count == 3)
                {
                    if (allCards[i].Rank >= allCards[index].Rank)
                        index = i;                
                }
            }

            if (index == 0) return;

            value += (int)allCards[index].Rank * 3;
            lastValue = (int)allCards[index].Rank;
            counter++;
            winnerCards.AddRange(allCards.Where(c => c.Rank == allCards[index].Rank).Select(card => card));
        }
    }
}

// using System.Collections.Generic;
// using System.Linq;
// using PokerHand.BusinessLogic.CardEvaluator.Interfaces;
// using PokerHand.Common.Entities;
// using PokerHand.Common.Helpers;
// using PokerHand.Common.Helpers.Card;
//
// namespace PokerHand.BusinessLogic.CardEvaluator.Hands
// {
//     public class FullHouse : IRules
//     {
//         private const int Rate = 6700;
//
//         public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame)
//         {
//             var result = new EvaluationResult();
//             
//             var allCards = tableCards.Concat(playerHand).ToList();
//            
//             JokerCheck(isJokerGame, allCards);
//
//             var lastValue = -1;
//             var counter = 0;
//
//             CheckOnThreeCard(ref value, allCards, ref lastValue, ref counter, ref finalCardsList);
//             
//             if (lastValue != -1) 
//                 allCards.RemoveAll(c => (int)c.Rank == lastValue);
//             
//             CheckOnTwoCards(ref value, allCards, ref counter, ref finalCardsList);
//
//             var isFullHouse = false;
//             handType = HandType.None;
//             if (counter == 2)
//             {
//                 isFullHouse = true;
//                 value *= Rate;
//                 handType = HandType.FullHouse;
//             }
//             else
//             {
//                 value = 0;
//                 finalCardsList = null;
//             }
//                 
//
//             return isFullHouse;
//         }
//
//         private void JokerCheck(bool isJokerGame, List<Card> cards)
//         {
//             if (!isJokerGame) return;
//
//             var numberOfJokers = cards.Where(card => card.Rank == CardRankType.Joker).Select(card => card).Count();
//
//             if (numberOfJokers == 1)
//                 OneJokerCheck(cards);
//             else
//                 TwoJokerCheck(cards);
//         }
//
//         private void OneJokerCheck(List<Card> cards)
//         {
//             for (var i = 0; i < cards.Count; i++)
//             {
//                 if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 3) continue;
//                 if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 2)
//                 {
//                     foreach (var card in cards)
//                         if (card.Rank == CardRankType.Joker)
//                             card.Rank = cards[i].Rank;
//                     break;
//                 }
//                 
//                 foreach (var card in cards)
//                     if (card.Rank == CardRankType.Joker)
//                         card.Rank = (CardRankType)GetMaxValue(cards);
//             }
//         }
//         
//         private void TwoJokerCheck(List<Card> cards)
//         {
//             for (var i = 0; i < cards.Count; i++)
//             {
//                 if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 3) continue;
//                 if (cards.FindAll(c => c.Rank == cards[i].Rank).Count == 2)
//                 {
//                     foreach (var card in cards)
//                         if (card.Rank == CardRankType.Joker)
//                             card.Rank = cards[i].Rank;
//                     break;
//                 }
//                 foreach (var card in cards)
//                     if (card.Rank == CardRankType.Joker)
//                         card.Rank = (CardRankType)GetMaxValue(cards);
//             }
//         }
//         
//         private int GetMaxValue(List<Card> cards)
//         {
//             var maxValue = 0;
//             
//             for (var i = 1; i < cards.Count; i++)
//             {
//                 if (maxValue < (int)cards[i].Rank)
//                 {
//                     if (cards[i].Rank != CardRankType.Joker)
//                         maxValue = (int)cards[i].Rank;
//                 }
//             }
//             
//             return maxValue;
//         }
//         
//         private void CheckOnTwoCards(ref int value, List<Card> allCards, ref int counter, ref List<Card> winnerCards)
//         {
//             var index = 0;
//             for (var i = 0; i < allCards.Count; i++)
//             {
//                 if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 2)
//                 {
//                     index = i;
//                     break;
//                 }
//             }
//             for (var i = 0; i < allCards.Count; i++)
//             {
//                 if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 2)
//                 {
//                     if (allCards[i].Rank >= allCards[index].Rank)
//                         index = i;
//                 }
//             }
//
//             if (index == 0) return;
//
//             value += (int)allCards[index].Rank * 2;
//             counter++;
//             winnerCards.AddRange(allCards.Where(c => c.Rank == allCards[index].Rank).Select(item => item));
//
//             //Старая рабочая версия
//             //foreach (Card currentCard in temp)
//             //{
//             //    if (temp.FindAll(Card => Card.value == currentCard.value).Count >= 2)
//             //    {
//             //        Value += (int)currentCard.value * 2;
//             //        counter++;
//             //        newTemp.AddRange(temp.Where(g => g.value == currentCard.value).Select(item => item));
//             //        break;
//             //    }
//             //}
//         }
//
//         private void CheckOnThreeCard(ref int value, List<Card> allCards, ref int lastValue, ref int counter, ref List<Card> winnerCards)
//         {
//             var index = 0;
//             for (var i = 0; i < allCards.Count; i++)
//             {
//                 if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count >= 3)
//                 {
//                     index = i;
//                     break;
//                 }
//             }
//             for (int i = 0; i < allCards.Count; i++)
//             {
//                 if (allCards.FindAll(c => c.Rank == allCards[i].Rank).Count == 3)
//                 {
//                     if (allCards[i].Rank >= allCards[index].Rank)
//                         index = i;                
//                 }
//             }
//
//             if (index == 0) return;
//
//             value += (int)allCards[index].Rank * 3;
//             lastValue = (int)allCards[index].Rank;
//             counter++;
//             winnerCards.AddRange(allCards.Where(c => c.Rank == allCards[index].Rank).Select(card => card));
//         }
//     }
// }