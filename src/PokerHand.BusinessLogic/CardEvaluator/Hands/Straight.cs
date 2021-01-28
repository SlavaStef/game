using System.Collections.Generic;
using System.Linq;
using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;

namespace PokerHand.BusinessLogic.HandEvaluator.Hands
{
    public class Straight : IRules
    {
        private const int Rate = 400;
        
        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
        {
            var isStraight = false;
            finalCardsList = new List<Card>(5);
            var allCards = tableCards.Concat(playerHand).ToList();

            CardEvaluator.SortByRankAscending(allCards);

            if (isJokerGame)
                allCards = CheckJokers(allCards);
            
            isStraight = MainStraightCheck(allCards, finalCardsList);

            if(!isStraight)
                isStraight = CheckOnLowAceStraight(allCards, finalCardsList);
            
            if (isStraight)
            {
                CardEvaluator.SortByRankAscending(finalCardsList);
                value = GetScore(finalCardsList);
                handType = HandType.Straight;
            }
            else
            {
                value = 0;
                handType = HandType.None;
            }
            
            return isStraight;
        }
        
        private bool MainStraightCheck(List<Card> allCards, List<Card> straightCards)
        {
            var numberOfDistinctRanks = allCards.Select(c => c.Rank).Distinct().Count();
            
            if (numberOfDistinctRanks > 4)
            {
                for (var i = numberOfDistinctRanks - 1; i >= 4; i--)
                {
                    var isStraight = (int)allCards[i].Rank == (int)allCards[i - 1].Rank + 1 
                                  && (int)allCards[i].Rank == (int)allCards[i - 2].Rank + 2 
                                  && (int)allCards[i].Rank == (int)allCards[i - 3].Rank + 3 
                                  && (int)allCards[i].Rank == (int)allCards[i - 4].Rank + 4;
                    
                    if (isStraight)
                    {
                        for(var j = i; j >= i - 4; j--)
                            straightCards.Add(allCards[j]);

                        return true;
                    }
                }
            }
            
            return false;        
        }
        
        private bool CheckOnLowAceStraight(List<Card> allCards, List<Card> straightCards)
        {
            if (allCards.Count(c => c.Rank == CardRankType.Ace) > 0
                && allCards.Count(c => c.Rank == CardRankType.Deuce) > 0
                && allCards.Count(c => c.Rank == CardRankType.Three) > 0
                && allCards.Count(c => c.Rank == CardRankType.Four) > 0
                && allCards.Count(c => c.Rank == CardRankType.Five) > 0)
            {
                straightCards.Add(allCards.First(c => c.Rank == CardRankType.Ace));
                straightCards.Add(allCards.First(c => c.Rank == CardRankType.Deuce));
                straightCards.Add(allCards.First(c => c.Rank == CardRankType.Three));
                straightCards.Add(allCards.First(c => c.Rank == CardRankType.Four));
                straightCards.Add(allCards.First(c => c.Rank == CardRankType.Five));

                return true;
            }

            return false;
        }
        
        private List<Card> CheckJokers(List<Card> allCards)
        {
            var numberOfJokers = allCards.Count(card => card.Rank == CardRankType.Joker);
            
            if (numberOfJokers == 1)
                allCards = CheckWithOneJoker(allCards);
            if (numberOfJokers == 2)
            {
                //Должно быть так
                //temp = CheckWithTwoJokers(temp);

                //А это костыль
                allCards = CheckWithOneJoker(allCards);
                allCards = CheckWithOneJoker(allCards);
            }
            
            return allCards;
        }
        
        private List<Card> CheckWithOneJoker(List<Card> allCards)
        {
            
            // НА ВХОДЕ:
            // var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart};
            // var card2 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade};
            // var card3 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            // var card4 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
            // var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
            // var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            // var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            
            var distinctRanks = allCards.Select(card => card.Rank).Distinct().ToList();
            var resultCards = new List<Card>(5);

            // Если значений 5, 6 или 7
            if (distinctRanks.Count > 4)
            {
                for (var i = distinctRanks.Count - 2; i > 2; i--)
                {
                    var subtraction = distinctRanks[i] - distinctRanks[i - 3];
                    if (subtraction == 4 || subtraction == 3)
                    {
                        var newDistinctTemp = distinctRanks.GetRange(i - 3, 4);
                        // var card3 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
                        // var card4 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
                        // var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
                        // var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
                        
                        // Добавляет в result объекты Card с неповторяющимися значениями карт
                        resultCards.AddRange(distinctRanks.Select(newValue => new Card {Rank = newValue}));

                        TransformJokerIntoCard(resultCards, allCards);
                        break;
                    }
                }
            }
            return resultCards;
        }

        private List<Card> CheckWithTwoJokers(List<Card> temp)
        {
            List<Card> result = new List<Card>(0);
            var distinctTemp = temp.Select(card => card.Rank).Distinct().ToList();

            //НУЖНО ДОДЕЛАТЬ, А НЕ УДАЛЯТЬ!!!!!
            //if (distinctTemp.Count > 4)
            //{
            //    if (distinctTemp.Count == 5 && distinctTemp[2] - distinctTemp[0] < distinctTemp.Count)
            //    {
            //        SetNewResult(result, distinctTemp);
            //    }
            //    else if (distinctTemp.Count == 6)
            //    {
            //        if (distinctTemp[3] - distinctTemp[1] < 4)
            //        {
            //            result = distinctTemp.GetRange(1, 3);
            //            result.Add(17);
            //            result.Add(17);
            //        }
            //        else if (distinctTemp[2] - distinctTemp[0] < 4)
            //        {
            //            result = distinctTemp.GetRange(0, 3);
            //            result.Add(17);
            //            result.Add(17);
            //        }
            //    }

            //    if (distinctTemp.Count == 7)
            //    {
            //        for (int i = distinctTemp.Count - 3; i > 1; i--)
            //        {
            //            var subtraction = distinctTemp[i] - distinctTemp[i - 2];
            //            if (subtraction < 5)
            //            {
            //                var newDistinctTemp = distinctTemp.GetRange(i - 2, 3);
            //                SetNewResult(result, distinctTemp);
            //                JokerChange(result, temp);
            //                break;
            //            }
            //        }
            //    }
            //}
            return result;
        }

        private void SetNewResult(List<Card> result, List<CardRankType> distinctRanks)
        {
            foreach (CardRankType newValue in distinctRanks)
            {
                result.Add(new Card
                {
                    Rank = newValue
                });
            }
        }

        private void TransformJokerIntoCard(List<Card> resultCards, List<Card> allCards)
        {
            // Get joker cards
            foreach (var card in allCards.Where(card => card.Rank == CardRankType.Joker))
            {
                // Iterate through a list of cards with distinct ranks 
                for (var i = 0; i < resultCards.Count - 1; i++)
                {
                    if (resultCards[i].Rank + 1 == resultCards[i + 1].Rank) 
                        continue;
                    
                    var newCard = new Card
                    {
                        Rank = resultCards[i].Rank + 1
                    };

                    if (newCard.Rank > CardRankType.Ace)
                    {
                        newCard.Rank = resultCards[0].Rank - 1;
                        resultCards.Add(newCard);
                    }
                    else
                        resultCards.Add(newCard);
                    break;
                }
            }
        }
        
        private int GetScore(List<Card> cards)
        {
            var count = 0;
            if (cards.Count > 4)
            {
                foreach (var card in cards)
                    count += (int)card.Rank;

                count *= Rate;
            }
            
            return count;
        }
    }
}