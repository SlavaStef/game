using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic
{
    public class TwoPairs : IRules
    {
        private const int Rate = 17;

        public EvaluationResult Check(List<Card> playerHand, List<Card> tableCards)
        {
            var result = new EvaluationResult();

            var allCards = tableCards.Concat(playerHand).ToList();
            var numberOfJokers = allCards.Count(c => c.Rank is CardRankType.Joker);

            var numberOfPairs = 0;
            
            switch (numberOfJokers)
            {
                case 0:
                    result.Hand.Cards = new List<Card>(5);
                    
                    for (var counter = 0; counter < 2; counter++)
                    {
                        foreach (var card in allCards.Where(card => allCards.Count(c => c.Rank == card.Rank) is 2))
                        {
                            numberOfPairs++;
                        
                            result.Hand.Value += (int)card.Rank * 2 * Rate;
                        
                            var cardsToAdd = allCards
                                .Where(c => c.Rank == card.Rank)
                                .ToArray();
                                
                            result.Hand.Cards.AddRange(cardsToAdd);
                        
                            allCards.RemoveAll(c => cardsToAdd.Contains(c));
                            break;
                        }

                        if (numberOfPairs is 0)
                            break;
                    }

                    if (numberOfPairs is 2)
                    {
                        result.IsWinningHand = true;
                        result.Hand.HandType = HandType.TwoPairs;

                        result.Hand.Cards = result.Hand.Cards
                            .OrderByDescending(c => c.Rank)
                            .ToList();
                        
                        AddSideCards(result.Hand.Cards, allCards);
                        result.Hand.Value += (int)result.Hand.Cards[4].Rank;
                    }
                    else
                    {
                        result.Hand.Cards = null;
                        result.Hand.Value = 0;
                    }

                    return result;
                case 1:
                    result.Hand.Cards = new List<Card>(5);
                    
                    for (var counter = 0; counter < 2; counter++)
                    {
                        foreach (var card in allCards.Where(card => allCards.Count(c => c.Rank == card.Rank) is 2))
                        {
                            numberOfPairs++;
                        
                            result.Hand.Value += (int)card.Rank * 2 * Rate;
                        
                            var cardsToAdd = allCards
                                .Where(c => c.Rank == card.Rank)
                                .ToArray();
                                
                            result.Hand.Cards.AddRange(cardsToAdd);
                        
                            allCards.RemoveAll(c => cardsToAdd.Contains(c));
                            break;
                        }

                        if (numberOfPairs is 0)
                            break;
                    }

                    switch (numberOfPairs)
                    {
                        case 0:
                            result.Hand.Cards = null;
                            result.Hand.Value = 0;

                            return result;
                        case 1:
                            result.IsWinningHand = true;
                            result.Hand.HandType = HandType.TwoPairs;

                            var maxCard = allCards
                                .Where(c => c.Rank is not CardRankType.Joker)
                                .OrderByDescending(c => c.Rank)
                                .First();
                        
                            result.Hand.Cards.Add(maxCard);
                            allCards.Remove(maxCard);

                            var joker = allCards.First(c => c.Rank is CardRankType.Joker);
                            joker.SubstitutedCard = new Card {Rank = maxCard.Rank};
                            result.Hand.Cards.Add(joker);
                            allCards.Remove(joker);

                            result.Hand.Value += (int) maxCard.Rank * 2 * Rate;

                            if (result.Hand.Cards[0].Rank < result.Hand.Cards[2].Rank)
                            {
                                var tempList = new List<Card>
                                {
                                    result.Hand.Cards[2],
                                    result.Hand.Cards[3],
                                    result.Hand.Cards[0],
                                    result.Hand.Cards[1]
                                };
                                
                                result.Hand.Cards = tempList.ToList();
                            }
                        
                            AddSideCards(result.Hand.Cards, allCards);
                            result.Hand.Value += (int)result.Hand.Cards.Last().Rank;
                            
                            return result;
                        case 2:
                            result.IsWinningHand = true;
                            result.Hand.HandType = HandType.TwoPairs;
                            
                            if (result.Hand.Cards[0].Rank < result.Hand.Cards[2].Rank)
                            {
                                var tempList = new List<Card>
                                {
                                    result.Hand.Cards[2],
                                    result.Hand.Cards[3],
                                    result.Hand.Cards[0],
                                    result.Hand.Cards[1]
                                };
                                
                                result.Hand.Cards = tempList.ToList();
                            }

                            var maxRemainedCard = allCards
                                .Where(c => c.Rank is not CardRankType.Joker)
                                .OrderByDescending(c => c.Rank).First();

                            if (result.Hand.Cards[0].Rank < maxRemainedCard.Rank ||
                                result.Hand.Cards[2].Rank < maxRemainedCard.Rank)
                            {
                                var firstCardToReturn = result.Hand.Cards[2];
                                var secondCardToReturn = result.Hand.Cards[3];
                                
                                allCards.Add(firstCardToReturn);
                                allCards.Add(secondCardToReturn);

                                result.Hand.Cards.Remove(firstCardToReturn);
                                result.Hand.Value -= (int)firstCardToReturn.Rank * 2 * Rate;
                                result.Hand.Cards.Remove(secondCardToReturn);
                                
                                result.Hand.Cards.Add(maxRemainedCard);
                                var jokerToAdd = allCards.First(c => c.Rank is CardRankType.Joker);
                                jokerToAdd.SubstitutedCard = new Card {Rank = maxRemainedCard.Rank};
                                result.Hand.Cards.Add(jokerToAdd);

                                result.Hand.Value += (int) maxRemainedCard.Rank * 2 * Rate;

                                allCards.Remove(maxRemainedCard);
                                allCards.Remove(jokerToAdd);
                            }
                            
                            if (result.Hand.Cards[0].Rank < result.Hand.Cards[2].Rank)
                            {
                                var tempList = new List<Card>
                                {
                                    result.Hand.Cards[2],
                                    result.Hand.Cards[3],
                                    result.Hand.Cards[0],
                                    result.Hand.Cards[1]
                                };
                                
                                result.Hand.Cards = tempList.ToList();
                            }

                            AddSideCards(result.Hand.Cards, allCards);
                            result.Hand.Value += (int)result.Hand.Cards[4].Rank;
                            
                            return result;
                    }
                    
                    return result;
            }
            
            return result;
        }
        
        private void AddSideCards(List<Card> finalCardsList, List<Card> allCards)
        {
            allCards = allCards
                .Where(c => c.Rank is not CardRankType.Joker)
                .OrderByDescending(c => c.Rank)
                .ToList();
            
            finalCardsList.Add(allCards[0]);
        }
    }
}