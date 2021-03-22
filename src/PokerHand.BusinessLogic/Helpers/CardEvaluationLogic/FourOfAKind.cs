using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic
{
    public class FourOfAKind : IRules
    {
        private const int Rate = 60000;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            
            var allCards = tableCards.Concat(playerHand).ToList();
            var dict = new Dictionary<CardRankType, int>();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);
            
            foreach (var card in allCards)
            {
                if (!dict.ContainsKey(card.Rank))
                    dict.Add(card.Rank, 1);
                else
                    dict[card.Rank]++;
            }

            if (numberOfJokers > 0)
            {
                switch (numberOfJokers)
                {
                    case 1:
                        // ThreeOfAKind + Joker + SideCard
                        var threeOfAKindCheck = CheckForThreeOfAKind(allCards);

                        if (threeOfAKindCheck.isThreeOfAKind)
                        {
                            result.IsWinningHand = true;
                            result.Hand.HandType = HandType.FourOfAKind;
                            result.Hand.Cards = new List<Card>();
                    
                            result.Hand.Cards.AddRange(threeOfAKindCheck.threeCards);
                            result.Hand.Value += (int) threeOfAKindCheck.threeCards[0].Rank * 4 * Rate;

                            var joker = allCards.First(c => c.Rank is CardRankType.Joker);
                            joker.SubstitutedCard = new Card {Rank = threeOfAKindCheck.threeCards[0].Rank};
                            result.Hand.Cards.Add(joker);
                            
                            var side = allCards
                                .Except(result.Hand.Cards)
                                .OrderByDescending(c => c.Rank)
                                .First();
                            
                            result.Hand.Cards.Add(side);
                            result.Hand.Value += (int) side.Rank * Rate;

                            return result;
                        }

                        return result;
                    case 2:
                        // OnePair + Joker + Joker + SideCard
                        var onePairCheck = CheckForOnePair(allCards);

                        if (onePairCheck.isOnePair)
                        {
                            result.IsWinningHand = true;
                            result.Hand.HandType = HandType.FourOfAKind;
                            result.Hand.Cards = new List<Card>();
                    
                            result.Hand.Cards.AddRange(onePairCheck.onePair);
                            result.Hand.Value += (int) onePairCheck.onePair[0].Rank * 4 * Rate;

                            var jokers = allCards.Where(c => c.Rank is CardRankType.Joker).ToList();

                            foreach (var card in jokers)
                                card.SubstitutedCard = new Card {Rank = result.Hand.Cards[0].Rank};
                            
                            result.Hand.Cards.AddRange(jokers);
                            
                            var side = allCards
                                .Except(result.Hand.Cards)
                                .OrderByDescending(c => c.Rank)
                                .First();
                            
                            result.Hand.Cards.Add(side);
                            result.Hand.Value += (int) side.Rank * Rate;

                            return result;
                        }

                        return result;
                }
            }

            if (dict.Any(c => c.Value is 4) is false)
                return result;

            result.IsWinningHand = true;
            result.Hand.HandType = HandType.FourOfAKind;
            result.Hand.Cards = new List<Card>();

            var winningRank = dict
                .First(c => c.Value is 4)
                .Key;

            var winningCards = allCards
                .Where(c => c.Rank == winningRank)
                .OrderByDescending(c => c.Rank)
                .Take(4)
                .ToList();

            var sideCard = allCards
                .Where(c => c.Rank != winningRank)
                .OrderByDescending(c => c.Rank)
                .First();
            
            result.Hand.Cards.AddRange(winningCards);
            result.Hand.Cards.Add(sideCard);
            result.Hand.Value = ((int)winningCards[0].Rank * 4 + (int)sideCard.Rank) * Rate;

            return result;
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
    }
}
