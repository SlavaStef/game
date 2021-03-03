using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.Helpers.CardEvaluationLogic.Hands
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
                    result.EvaluatedHand.Cards = new List<Card>(5);
                    
                    for (var counter = 0; counter < 2; counter++)
                    {
                        foreach (var card in allCards.Where(card => allCards.Count(c => c.Rank == card.Rank) is 2))
                        {
                            numberOfPairs++;
                        
                            result.EvaluatedHand.Value += (int)card.Rank * 2 * Rate;
                        
                            var cardsToAdd = allCards
                                .Where(c => c.Rank == card.Rank)
                                .ToArray();
                                
                            result.EvaluatedHand.Cards.AddRange(cardsToAdd);
                        
                            allCards.RemoveAll(c => cardsToAdd.Contains(c));
                            break;
                        }

                        if (numberOfPairs is 0)
                            break;
                    }

                    if (numberOfPairs is 2)
                    {
                        result.IsWinningHand = true;
                        result.EvaluatedHand.HandType = HandType.TwoPairs;

                        result.EvaluatedHand.Cards = result.EvaluatedHand.Cards
                            .OrderByDescending(c => c.Rank)
                            .ToList();
                        
                        AddSideCards(result.EvaluatedHand.Cards, allCards);
                        result.EvaluatedHand.Value += (int)result.EvaluatedHand.Cards[4].Rank;
                    }
                    else
                    {
                        result.EvaluatedHand.Cards = null;
                        result.EvaluatedHand.Value = 0;
                    }

                    return result;
                case 1:
                    result.EvaluatedHand.Cards = new List<Card>(5);
                    
                    for (var counter = 0; counter < 2; counter++)
                    {
                        foreach (var card in allCards.Where(card => allCards.Count(c => c.Rank == card.Rank) is 2))
                        {
                            numberOfPairs++;
                        
                            result.EvaluatedHand.Value += (int)card.Rank * 2 * Rate;
                        
                            var cardsToAdd = allCards
                                .Where(c => c.Rank == card.Rank)
                                .ToArray();
                                
                            result.EvaluatedHand.Cards.AddRange(cardsToAdd);
                        
                            allCards.RemoveAll(c => cardsToAdd.Contains(c));
                            break;
                        }

                        if (numberOfPairs is 0)
                            break;
                    }

                    switch (numberOfPairs)
                    {
                        case 0:
                            result.EvaluatedHand.Cards = null;
                            result.EvaluatedHand.Value = 0;

                            return result;
                        case 1:
                            result.IsWinningHand = true;
                            result.EvaluatedHand.HandType = HandType.TwoPairs;

                            var maxCard = allCards
                                .Where(c => c.Rank is not CardRankType.Joker)
                                .OrderByDescending(c => c.Rank)
                                .First();
                        
                            result.EvaluatedHand.Cards.Add(maxCard);
                            allCards.Remove(maxCard);

                            var joker = allCards.First(c => c.Rank is CardRankType.Joker);
                            joker.SubstitutedCard = new Card {Rank = maxCard.Rank};
                            result.EvaluatedHand.Cards.Add(joker);
                            allCards.Remove(joker);

                            result.EvaluatedHand.Value += (int) maxCard.Rank * 2 * Rate;

                            if (result.EvaluatedHand.Cards[0].Rank < result.EvaluatedHand.Cards[2].Rank)
                            {
                                var tempList = new List<Card>
                                {
                                    result.EvaluatedHand.Cards[2],
                                    result.EvaluatedHand.Cards[3],
                                    result.EvaluatedHand.Cards[0],
                                    result.EvaluatedHand.Cards[1]
                                };
                                
                                result.EvaluatedHand.Cards = tempList.ToList();
                            }
                        
                            AddSideCards(result.EvaluatedHand.Cards, allCards);
                            result.EvaluatedHand.Value += (int)result.EvaluatedHand.Cards.Last().Rank;
                            
                            return result;
                        case 2:
                            result.IsWinningHand = true;
                            result.EvaluatedHand.HandType = HandType.TwoPairs;
                            
                            if (result.EvaluatedHand.Cards[0].Rank < result.EvaluatedHand.Cards[2].Rank)
                            {
                                var tempList = new List<Card>
                                {
                                    result.EvaluatedHand.Cards[2],
                                    result.EvaluatedHand.Cards[3],
                                    result.EvaluatedHand.Cards[0],
                                    result.EvaluatedHand.Cards[1]
                                };
                                
                                result.EvaluatedHand.Cards = tempList.ToList();
                            }

                            var maxRemainedCard = allCards
                                .Where(c => c.Rank is not CardRankType.Joker)
                                .OrderByDescending(c => c.Rank).First();

                            if (result.EvaluatedHand.Cards[0].Rank < maxRemainedCard.Rank ||
                                result.EvaluatedHand.Cards[2].Rank < maxRemainedCard.Rank)
                            {
                                var firstCardToReturn = result.EvaluatedHand.Cards[2];
                                var secondCardToReturn = result.EvaluatedHand.Cards[3];
                                
                                allCards.Add(firstCardToReturn);
                                allCards.Add(secondCardToReturn);

                                result.EvaluatedHand.Cards.Remove(firstCardToReturn);
                                result.EvaluatedHand.Value -= (int)firstCardToReturn.Rank * 2 * Rate;
                                result.EvaluatedHand.Cards.Remove(secondCardToReturn);
                                
                                result.EvaluatedHand.Cards.Add(maxRemainedCard);
                                var jokerToAdd = allCards.First(c => c.Rank is CardRankType.Joker);
                                jokerToAdd.SubstitutedCard = new Card {Rank = maxRemainedCard.Rank};
                                result.EvaluatedHand.Cards.Add(jokerToAdd);

                                result.EvaluatedHand.Value += (int) maxRemainedCard.Rank * 2 * Rate;

                                allCards.Remove(maxRemainedCard);
                                allCards.Remove(jokerToAdd);
                            }
                            
                            if (result.EvaluatedHand.Cards[0].Rank < result.EvaluatedHand.Cards[2].Rank)
                            {
                                var tempList = new List<Card>
                                {
                                    result.EvaluatedHand.Cards[2],
                                    result.EvaluatedHand.Cards[3],
                                    result.EvaluatedHand.Cards[0],
                                    result.EvaluatedHand.Cards[1]
                                };
                                
                                result.EvaluatedHand.Cards = tempList.ToList();
                            }

                            AddSideCards(result.EvaluatedHand.Cards, allCards);
                            result.EvaluatedHand.Value += (int)result.EvaluatedHand.Cards[4].Rank;
                            
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