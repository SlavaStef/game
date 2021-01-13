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

            SortByRank(allCards);
            
            isStraight = MainStraightCheck(CheckOnJoker(allCards, isJokerGame), finalCardsList);

            if(!isStraight)
                isStraight = CheckOnLowAceStraight(allCards, finalCardsList);
            
            if (isStraight)
            {
                SortByRank(finalCardsList);
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
        
        private List<Card> CheckOnJoker(List<Card> allCards, bool isJokerGame)
        {
            if (!isJokerGame) return allCards;

            //Подсчитываем количество джокеров в колоде
            int jokerCount = 0;
            foreach (Card card in allCards)        
                if (card.Rank == CardRankType.Joker)
                    jokerCount++;

            if (jokerCount == 2)
            {
                //Должно быть так
                //temp = CountTwoJoker(temp);

                //А это костыль
                allCards = CountOneJoker(allCards);
                allCards = CountOneJoker(allCards);
            }
            else if (jokerCount == 1)
                allCards = CountOneJoker(allCards);
            else
                return allCards;

            return allCards;
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

        private static void SortByRank(List<Card> cards)
        {
            for (var i = 0; i < cards.Count - 1; i++)
            {
                for (var j = i + 1; j < cards.Count; j++)
                {
                    if (cards[i].Rank > cards[j].Rank)
                    {
                        var tempCard = cards[i];
                        cards[i] = cards[j];
                        cards[j] = tempCard;
                    }
                }
            }
        }
        
        private List<Card> CountOneJoker(List<Card> allCards)
        {
            List<Card> result = new List<Card>(0);
            //Выбрать неповторяющиеся значения карт
            var distinctRanks = allCards.Select(card => card.Rank).Distinct().ToList();

            // Если значений 5, 6 или 7
            if (distinctRanks.Count > 4)
            {
                for (int i = distinctRanks.Count - 2; i > 2; i--)
                {
                    var subtraction = distinctRanks[i] - distinctRanks[i - 3];
                    if (subtraction == 4 || subtraction == 3)
                    {
                        var newDistinctTemp = distinctRanks.GetRange(i - 3, 4);
                        
                        // Добавляет в result объекты Card с неповторяющимися значениями карт
                        foreach (CardRankType newValue in distinctRanks)
                        {
                            result.Add(new Card { Rank = newValue });
                        }
                        
                        JokerChange(result, allCards);
                        break;
                    }
                }
            }
            return result;
        }

        private List<Card> CountTwoJoker(List<Card> temp)
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

        private void JokerChange(List<Card> result, List<Card> temp)
        {
            //Если два джокера, то один из них может оказаться с таким же value как и второй
            //но это не точно
            foreach (Card card in temp)
            {
                if (card.Rank == CardRankType.Joker)
                {
                    for (int j = 0; j < result.Count - 1; j++)
                    {
                        if (result[j].Rank + 1 == result[j + 1].Rank) continue;
                        Card newCard = new Card { Rank = result[j].Rank + 1 };

                        if (newCard.Rank > CardRankType.Ace)
                        {
                            newCard.Rank = result[0].Rank - 1;
                            result.Add(newCard);
                        }
                        else
                            result.Add(newCard);
                        break;
                    }
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