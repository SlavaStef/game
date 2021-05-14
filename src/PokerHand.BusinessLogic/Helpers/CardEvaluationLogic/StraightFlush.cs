using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic
{
    public class StraightFlush : IRules
    {
        private const int StraightFlushRate = 180_000;
        private const int RoyalFlushRate = 200_000;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();
            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);
            var jokers = allCards.Where(c => c.Rank is CardRankType.Joker).ToList();

            allCards = allCards.Where(c => c.Rank is not CardRankType.Joker)
                .GroupBy(c => c.Rank)
                .Select(g => g.First())
                .OrderByDescending(c => c.Rank)
                .ToList();
            allCards.AddRange(jokers);

            if (allCards.Count < 5) return result;

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
        }

        private EvaluationResult CheckWithOneJoker(List<Card> playerHand, List<Card> tableCards, EvaluationResult result)
        {
            var allCards = tableCards.Concat(playerHand).ToList().OrderBy(c => c.Rank).ToList();

            var (isFiveStraight, fiveStraightHand) = CheckForFiveStraightCardsWithOneJoker(allCards);
            if (isFiveStraight)
            {
                var isAllSameSuit = CheckForSameSuits(fiveStraightHand);
                if (isAllSameSuit)
                {
                    if (CheckForFlashRoyal(fiveStraightHand))
                    {
                        result.IsWinningHand = true;
                        result.Hand = new Hand
                        {
                            Cards = fiveStraightHand,
                            HandType = HandType.RoyalFlush,
                            Value = EvaluateHand(fiveStraightHand, HandType.RoyalFlush)
                        };

                        return result;
                    }
                    
                    result.IsWinningHand = true;
                    result.Hand = new Hand
                    {
                        Cards = fiveStraightHand,
                        HandType = HandType.StraightFlush,
                        Value = EvaluateHand(fiveStraightHand, HandType.StraightFlush)
                    };

                    return result;
                }
            }
            
            var (isFourNotStraight, fourNotStraightHand) = CheckForFourNotStraightCardsWithOneJoker(allCards);
            if (isFourNotStraight)
            {
                var isAllSameSuit = CheckForSameSuits(fourNotStraightHand);
                if (isAllSameSuit)
                {
                    if (CheckForFlashRoyal(fourNotStraightHand))
                    {
                        result.IsWinningHand = true;
                        result.Hand = new Hand
                        {
                            Cards = fourNotStraightHand,
                            HandType = HandType.RoyalFlush,
                            Value = EvaluateHand(fourNotStraightHand, HandType.RoyalFlush)
                        };

                        return result;
                    }
                    
                    result.IsWinningHand = true;
                    result.Hand = new Hand
                    {
                        Cards = fourNotStraightHand,
                        HandType = HandType.StraightFlush,
                        Value = EvaluateHand(fourNotStraightHand, HandType.StraightFlush)
                    };

                    return result;
                }
            }
            
            var (isFourStraight, fourStraightHand) = CheckForFourStraightCardsWithOneJoker(allCards);
            if (isFourStraight)
            {
                var isAllSameSuit = CheckForSameSuits(fourStraightHand);
                if (isAllSameSuit)
                {
                    if (CheckForFlashRoyal(fourStraightHand))
                    {
                        result.IsWinningHand = true;
                        result.Hand = new Hand
                        {
                            Cards = fourStraightHand,
                            HandType = HandType.RoyalFlush,
                            Value = EvaluateHand(fourStraightHand, HandType.RoyalFlush)
                        };

                        return result;
                    }
                    
                    result.IsWinningHand = true;
                    result.Hand = new Hand
                    {
                        Cards = fourStraightHand,
                        HandType = HandType.StraightFlush,
                        Value = EvaluateHand(fourStraightHand, HandType.StraightFlush)
                    };

                    return result;
                }
            }

            return result;
        }
        
        private EvaluationResult CheckWithNoJokers(List<Card> playerHand, List<Card> tableCards, EvaluationResult result)
        {
            var isStraightResult = new Straight().Check(playerHand, tableCards);
            if (isStraightResult.IsWinningHand is false) return result;

            var (isStraightFlush, numberOfUsedJokers) = AnalyzeStraightCardsSuits(isStraightResult.Hand.Cards);

            if (isStraightFlush is false) return result;

            if (isStraightResult.Hand.Cards[0].Rank is CardRankType.Ace)
            {
                result.IsWinningHand = true;
                result.Hand.HandType = HandType.RoyalFlush;
                result.Hand.Cards = isStraightResult.Hand.Cards.ToList();
                
                foreach (var card in result.Hand.Cards) result.Hand.Value += (int) card.Rank * RoyalFlushRate;

                return result;
            }

            result.IsWinningHand = true;
            result.Hand.HandType = HandType.StraightFlush;
            result.Hand.Cards = isStraightResult.Hand.Cards.ToList();

            foreach (var card in result.Hand.Cards) result.Hand.Value += (int) card.Rank * StraightFlushRate;

            return result;
        }

        private static (bool, List<Card>) CheckForFiveStraightCardsWithOneJoker(IReadOnlyList<Card> allCards)
        {
            for (var index = allCards.Count - 6; index >= 0; index--)
            {
                var isFiveStraight = (int) allCards[index].Rank == (int) allCards[index + 1].Rank - 1 &&
                                     (int) allCards[index].Rank == (int) allCards[index + 2].Rank - 2 &&
                                     (int) allCards[index].Rank == (int) allCards[index + 3].Rank - 3 &&
                                     (int) allCards[index].Rank == (int) allCards[index + 4].Rank - 4;

                if (isFiveStraight is false) continue;

                if (allCards[index + 4].Rank is CardRankType.Ace)
                {
                    return (true,
                        new List<Card>
                        {
                            allCards[index + 4],
                            allCards[index + 3],
                            allCards[index + 2],
                            allCards[index + 1],
                            allCards[index]
                        });
                }

                allCards[6].SubstitutedCard = new Card
                {
                    Rank = allCards[index + 4].Rank + 1,
                    Suit = allCards[index + 4].Suit
                };

                return (true,
                    new List<Card>
                    {
                        allCards[6],
                        allCards[index + 4],
                        allCards[index + 3],
                        allCards[index + 2],
                        allCards[index + 1]
                    });
            }

            return (false, null);
        }

        private static (bool, List<Card>) CheckForFourStraightCardsWithOneJoker(IReadOnlyList<Card> allCards)
        {
            for (var index = allCards.Count - 5; index >= 0; index--)
            {
                var isFourStraight = (int) allCards[index].Rank == (int) allCards[index + 1].Rank - 1 &&
                                     (int) allCards[index].Rank == (int) allCards[index + 2].Rank - 2 &&
                                     (int) allCards[index].Rank == (int) allCards[index + 3].Rank - 3;

                if (isFourStraight is false) continue;

                if (allCards[index + 3].Rank is CardRankType.Ace)
                {
                    allCards[6].SubstitutedCard = new Card
                    {
                        Rank = CardRankType.Ten,
                        Suit = allCards[index + 3].Suit
                    };

                    return (true,
                        new List<Card>
                        {
                            allCards[index + 3],
                            allCards[index + 2],
                            allCards[index + 1],
                            allCards[index],
                            allCards[6]
                        });
                }

                allCards[6].SubstitutedCard = new Card
                {
                    Rank = allCards[index + 3].Rank + 1,
                    Suit = allCards[index + 3].Suit
                };

                return (true,
                    new List<Card>
                    {
                        allCards[6],
                        allCards[index + 3],
                        allCards[index + 2],
                        allCards[index + 1],
                        allCards[index]
                    });
            }

            return (false, null);
        }

        private static (bool, List<Card>) CheckForFourNotStraightCardsWithOneJoker(IReadOnlyList<Card> allCards)
        {
            for (var index = allCards.Count - 5; index >= 0; index--)
            {
                var isFourNotStraight = allCards[index].Rank == allCards[index + 3].Rank - 4;

                if (isFourNotStraight is false) continue;

                var result = new List<Card>();

                for (var i = index; i < index + 4; i++)
                {
                    result.Add(allCards[i]);

                    if ((int) allCards[i].Rank != (int) allCards[i + 1].Rank - 1 && result.Count != 5)
                    {
                        allCards[6].SubstitutedCard = new Card
                        {
                            Rank = allCards[i + 1].Rank - 1,
                            Suit = allCards[i].Suit
                        };

                        result.Add(allCards[6]);
                    }
                }

                result.Reverse();

                return (true, result);
            }

            return (false, null);
        }

        private (bool, int) AnalyzeStraightCardsSuits(List<Card> cards)
        {
            var cardsWithoutJokers = cards.Where(c => c.Rank is not CardRankType.Joker).ToList();
            
            if (cardsWithoutJokers.All(c => c.Suit == cardsWithoutJokers[0].Suit))
                return (true, cards.Count - cardsWithoutJokers.Count);

            return (false, cards.Count - cardsWithoutJokers.Count);
        }
        
        private static bool CheckForSameSuits(List<Card> cards)
        {
            var suitToCompare = cards.First(c => c.Rank is not CardRankType.Joker).Suit;

            var result = cards
                .Where(c => c.Rank is not CardRankType.Joker)
                .All(c => c.Suit == suitToCompare);

            return result;
        }
        
        private static int EvaluateHand(List<Card> cards, HandType handType)
        {
            var result = 0;
            var rate = handType is HandType.StraightFlush ? StraightFlushRate : RoyalFlushRate;

            foreach (var card in cards)
            {
                if (card.Rank is CardRankType.Joker)
                {
                    result += (int) card.SubstitutedCard.Rank * rate;
                    continue;
                }

                result += (int) card.Rank * rate;
            }

            return result;
        }

        private static bool CheckForFlashRoyal(List<Card> cards)
        {
            var isAceTop = cards[0].Rank is CardRankType.Ace;
            var isJokerTop = cards[0].Rank is CardRankType.Joker;
            var isJokerAceTop = cards[0].SubstitutedCard?.Rank is CardRankType.Ace;

            return isAceTop || isJokerTop && isJokerAceTop;
        }

    }
}