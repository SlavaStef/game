// using System.Collections.Generic;
// using PokerHand.BusinessLogic.HandEvaluator.Interfaces;
// using PokerHand.Common.Entities;
// using PokerHand.Common.Helpers;
//
// namespace PokerHand.BusinessLogic.HandEvaluator.Hands
// {
//     public class StraightFlush : IRules
//     {
//         private const int Rate = 180000;
//
//         public bool Check(List<Card> playerHand, List<Card> tableCards, bool isJokerGame, out int value, out HandType handType, out List<Card> finalCardsList)
//         {
//             var isStraight = new Straight().Check(playerHand, tableCards, isJokerGame, out int checkedValue,
//                 out HandType checkedHandType, out List<Card> listOfStraightCards);
//             var isFlush = listOfStraightCards.TrueForAll(c => c.Suit == listOfStraightCards[0].Suit);
//
//         
//             value = 0;
//             var isStraightFlush = false;
//             finalCardsList = new List<Card>();
//             
//             if (isStraight && isFlush)
//             {
//                 value = 0;
//                 if (listOfStraightCards.Count > 4)
//                 {
//                     foreach (var card in listOfStraightCards)
//                         value += (int)card.Rank;
//
//                     finalCardsList = listOfStraightCards;
//                     isStraightFlush = true;
//                 }
//             }
//
//             if (isStraightFlush)
//             {
//                 value *= Rate;
//                 handType = HandType.StraightFlush;
//             }
//             else
//             {
//                 value = 0;
//                 handType = HandType.None;
//             }
//         
//             return isStraightFlush;
//         }
//     }
// }
