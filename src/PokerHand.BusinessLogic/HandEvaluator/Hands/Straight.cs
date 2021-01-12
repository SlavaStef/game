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
        private bool _result;
        private List<Card> _newCards;

        public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
        {
            IsStraight(playerHand, tableCards, isJokerGame, out List<Card> cardsAfterCheck);
            totalCards = cardsAfterCheck;
            
            if (_result)
            {
                value = GetScore();
                handType = HandType.Straight;
            }
            else
            {
                value = 0;
                handType = HandType.None;
            }
            
            return _result;
        }

        public bool IsStraight(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out List<Card> cardsAfterCheck)
        {
            _newCards = new List<Card>(5);
            var allCards = tableCards.Concat(playerHand).ToList();

            // Sort by rank
            SortByRank(allCards);
            
            MainStraightCheck(CheckOnJoker(allCards, isJokerGame));
            
            // Проверка подрядидущих карт
            CheckOnLowAceStraight(allCards);

            cardsAfterCheck = _newCards;
            return _result;
        }
        
        private void SortByRank(List<Card> allCards)
        {
            var tempCard = new Card();
            
            for (var i = 0; i < allCards.Count - 1; i++)
            {
                for (var j = i + 1; j < allCards.Count; j++)
                {
                    if (allCards[i].Rank > allCards[j].Rank)
                    {
                        tempCard = allCards[i];
                        allCards[i] = allCards[j];
                        allCards[j] = tempCard;
                    }
                }
            }
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
        
        private void MainStraightCheck(List<Card> allCards)
        {
            var distinctRanks = allCards.Select(c => c.Rank).Distinct().ToList();
            
            // Если есть как минимум 5 карт с разными значениями, то
            if (distinctRanks.Count > 4)
            {
                // Проходим 3 раза
                for (int i = 0; i < distinctRanks.Count() - 4; i++)
                {
                    // проверяем, есть ли 5 карт подряд
                    _result = IsFiveCardsStraight(distinctRanks, i);
                    // если есть 5 карт, то
                    if (_result)
                    {
                        // добавляем карты в результат и прерываем цикл
                        AddToNewCard(allCards, distinctRanks, i);
                        break;
                    }
                }
            }
            // иначе не может быть straight
            else        
                _result = false;        
        }
        
        private bool IsFiveCardsStraight(List<CardRankType> ranks, int i)
        {
            return ranks[i] == ranks[i + 1] - 1
                   && ranks[i] == ranks[i + 2] - 2
                   && ranks[i] == ranks[i + 3] - 3
                   && ranks[i] == ranks[i + 4] - 4;
        }
        
        private void AddToNewCard(List<Card> temp, List<CardRankType> distinctTemp, int index)
        {
            for (int i = index; i < index + 6; i++)
            {
                _newCards.Add(temp[i]);
            }
        }
        
        private void CheckOnLowAceStraight(List<Card> temp)
        {
            if (_result)        
                return;
        
            if (temp[0].Rank == CardRankType.Ace
                && temp[temp.Count - 4].Rank == CardRankType.Five
                && temp[temp.Count - 3].Rank == CardRankType.Four
                && temp[temp.Count - 2].Rank == CardRankType.Three
                && temp[temp.Count - 1].Rank == CardRankType.Deuce)
            {
                _result = true;
                _newCards.Add(temp[0]);
                _newCards.Add(temp[temp.Count - 4]);
                _newCards.Add(temp[temp.Count - 3]);
                _newCards.Add(temp[temp.Count - 2]);
                _newCards.Add(temp[temp.Count - 1]);
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
        
        private int GetScore()
        {
            int count = 0;
            if (_newCards.Count > 4)
            {
                for (var i = 0; i < _newCards.Count; i++)
                    count += (int)_newCards[i].Rank;
                _result = true;
            }
            if (_result)
                count *= Rate;
            return count;
        }
    }
}

// using System.Collections.Generic;
// using System.Linq;
// using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
// using PokerHand.Common.Entities;
// using PokerHand.Common.Helpers;
// using PokerHand.Common.Helpers.Card;
//
// namespace PokerHand.BusinessLogic.HandEvaluator.Hands
// {
//     public class Straight : IRules
//     {
//         private const int Rate = 400;
//         private bool _result;
//         private List<Card> _newCards;
//
//         public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> totalCards)
//         {
//             IsStraight(playerHand, tableCards, isJokerGame, out List<Card> cardsAfterCheck);
//             totalCards = cardsAfterCheck;
//             
//             if (_result)
//             {
//                 value = GetScore();
//                 handType = HandType.Straight;
//             }
//             else
//             {
//                 value = 0;
//                 handType = HandType.None;
//             }
//             
//             return _result;
//         }
//
//         public bool IsStraight(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out List<Card> cardsAfterCheck)
//         {
//             _newCards = new List<Card>();
//             var allCards = tableCards.Concat(playerHand).ToList();
//
//             // Sort by rank
//             SortByRank(allCards);
//             
//             MainStraightCheck(CheckOnJoker(allCards, isJokerGame));
//             
//             // Проверка подрядидущих карт
//             CheckOnLowAceStraight(allCards);
//
//             cardsAfterCheck = _newCards;
//             return _result;
//         }
//         
//         private void SortByRank(List<Card> allCards)
//         {
//             var tempCard = new Card();
//             
//             for (var i = 0; i < allCards.Count - 1; i++)
//             {
//                 for (var j = i + 1; j < allCards.Count; j++)
//                 {
//                     if (allCards[i].Rank > allCards[j].Rank)
//                     {
//                         tempCard = allCards[i];
//                         allCards[i] = allCards[j];
//                         allCards[j] = tempCard;
//                     }
//                 }
//             }
//         }
//         
//         private List<Card> CheckOnJoker(List<Card> allCards, bool isJokerGame)
//         {
//             if (!isJokerGame) return allCards;
//
//             //Подсчитываем количество джокеров в колоде
//             int jokerCount = 0;
//             foreach (Card card in allCards)        
//                 if (card.Rank == CardRankType.Joker)
//                     jokerCount++;
//
//             if (jokerCount == 2)
//             {
//                 //Должно быть так
//                 //temp = CountTwoJoker(temp);
//
//                 //А это костыль
//                 allCards = CountOneJoker(allCards);
//                 allCards = CountOneJoker(allCards);
//             }
//             else if (jokerCount == 1)
//                 allCards = CountOneJoker(allCards);
//             else
//                 return allCards;
//
//             return allCards;
//         }
//         
//         private void MainStraightCheck(List<Card> allCards)
//         {
//             var distinctRanks = allCards.Select(c => c.Rank).Distinct().ToList();
//             
//             // Если есть как минимум 5 карт с разными значениями, то
//             if (distinctRanks.Count > 4)
//             {
//                 // Проходим 4 раза
//                 for (int i = 0; i < distinctRanks.Count() - 4; i++)
//                 {
//                     // проверяем, есть ли 5 карт подряд
//                     _result = IsFiveCardsStraight(distinctRanks, i);
//                     // если есть 5 карт, то
//                     if (_result)
//                     {
//                         // добавляем карты в результат и прерываем цикл
//                         AddToNewCard(allCards, i);
//                         break;
//                     }
//                 }
//             }
//             // иначе не может быть straight
//             else        
//                 _result = false;        
//         }
//         
//         private bool IsFiveCardsStraight(List<CardRankType> ranks, int i)
//         {
//             return ranks[i] == ranks[i + 1] - 1
//                    && ranks[i] == ranks[i + 2] - 2
//                    && ranks[i] == ranks[i + 3] - 3
//                    && ranks[i] == ranks[i + 4] - 4;
//         }
//         
//         private void AddToNewCard(List<Card> allCards, int index)
//         {
//             for (int i = index; i < index + 5; i++)
//             {
//                 _newCards.Add(allCards[i]);
//             }
//         }
//         
//         private void CheckOnLowAceStraight(List<Card> temp)
//         {
//             if (_result)        
//                 return;
//         
//             if (temp[0].Rank == CardRankType.Ace
//                 && temp[temp.Count - 4].Rank == CardRankType.Five
//                 && temp[temp.Count - 3].Rank == CardRankType.Four
//                 && temp[temp.Count - 2].Rank == CardRankType.Three
//                 && temp[temp.Count - 1].Rank == CardRankType.Deuce)
//             {
//                 _result = true;
//                 _newCards.Add(temp[0]);
//                 _newCards.Add(temp[temp.Count - 4]);
//                 _newCards.Add(temp[temp.Count - 3]);
//                 _newCards.Add(temp[temp.Count - 2]);
//                 _newCards.Add(temp[temp.Count - 1]);
//             }
//         }
//
//         
//         private List<Card> CountOneJoker(List<Card> allCards)
//         {
//             List<Card> result = new List<Card>(0);
//             //Выбрать неповторяющиеся значения карт
//             var distinctRanks = allCards.Select(card => card.Rank).Distinct().ToList();
//
//             // Если значений 5, 6 или 7
//             if (distinctRanks.Count > 4)
//             {
//                 for (int i = distinctRanks.Count - 2; i > 2; i--)
//                 {
//                     var subtraction = distinctRanks[i] - distinctRanks[i - 3];
//                     if (subtraction == 4 || subtraction == 3)
//                     {
//                         var newDistinctTemp = distinctRanks.GetRange(i - 3, 4);
//                         
//                         // Добавляет в result объекты Card с неповторяющимися значениями карт
//                         foreach (CardRankType newValue in distinctRanks)
//                         {
//                             result.Add(new Card { Rank = newValue });
//                         }
//                         
//                         JokerChange(result, allCards);
//                         break;
//                     }
//                 }
//             }
//             return result;
//         }
//
//         private List<Card> CountTwoJoker(List<Card> temp)
//         {
//             List<Card> result = new List<Card>(0);
//             var distinctTemp = temp.Select(card => card.Rank).Distinct().ToList();
//
//             //НУЖНО ДОДЕЛАТЬ, А НЕ УДАЛЯТЬ!!!!!
//             //if (distinctTemp.Count > 4)
//             //{
//             //    if (distinctTemp.Count == 5 && distinctTemp[2] - distinctTemp[0] < distinctTemp.Count)
//             //    {
//             //        SetNewResult(result, distinctTemp);
//             //    }
//             //    else if (distinctTemp.Count == 6)
//             //    {
//             //        if (distinctTemp[3] - distinctTemp[1] < 4)
//             //        {
//             //            result = distinctTemp.GetRange(1, 3);
//             //            result.Add(17);
//             //            result.Add(17);
//             //        }
//             //        else if (distinctTemp[2] - distinctTemp[0] < 4)
//             //        {
//             //            result = distinctTemp.GetRange(0, 3);
//             //            result.Add(17);
//             //            result.Add(17);
//             //        }
//             //    }
//
//             //    if (distinctTemp.Count == 7)
//             //    {
//             //        for (int i = distinctTemp.Count - 3; i > 1; i--)
//             //        {
//             //            var subtraction = distinctTemp[i] - distinctTemp[i - 2];
//             //            if (subtraction < 5)
//             //            {
//             //                var newDistinctTemp = distinctTemp.GetRange(i - 2, 3);
//             //                SetNewResult(result, distinctTemp);
//             //                JokerChange(result, temp);
//             //                break;
//             //            }
//             //        }
//             //    }
//             //}
//             return result;
//         }
//
//         private void SetNewResult(List<Card> result, List<CardRankType> distinctRanks)
//         {
//             foreach (CardRankType newValue in distinctRanks)
//             {
//                 result.Add(new Card
//                 {
//                     Rank = newValue
//                 });
//             }
//         }
//
//         private void JokerChange(List<Card> result, List<Card> temp)
//         {
//             //Если два джокера, то один из них может оказаться с таким же value как и второй
//             //но это не точно
//             foreach (Card card in temp)
//             {
//                 if (card.Rank == CardRankType.Joker)
//                 {
//                     for (int j = 0; j < result.Count - 1; j++)
//                     {
//                         if (result[j].Rank + 1 == result[j + 1].Rank) continue;
//                         Card newCard = new Card { Rank = result[j].Rank + 1 };
//
//                         if (newCard.Rank > CardRankType.Ace)
//                         {
//                             newCard.Rank = result[0].Rank - 1;
//                             result.Add(newCard);
//                         }
//                         else
//                             result.Add(newCard);
//                         break;
//                     }
//                 }
//             }
//         }
//         
//         private int GetScore()
//         {
//             int count = 0;
//             if (_newCards.Count > 4)
//             {
//                 foreach (var card in _newCards)
//                     count += (int)card.Rank;
//
//                 _result = true;
//             }
//             if (_result)
//                 count *= Rate;
//             return count;
//         }
//     }
// }