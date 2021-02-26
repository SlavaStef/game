using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluator.Hands;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;
using Xunit;

namespace PokerHand.BusinessLogic.Tests
{
    public class HandEvaluatorTests
    {
        #region High Card
        // HIGH CARD
        // High card is a hand that does not fall into any other category
        // High card with Joker is impossible
        
        [Fact]
        public void HighCard_NoJoker_ReturnsTrue()
        {
            // Arrange
            var fullHouse = new HighCard();

            var card1 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card2 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart};
            var card3 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
            var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             
            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var expectedResult = new List<Card> {card6, card3, card7, card1, card2};

            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards);
             
            // Assert
            result.IsWinningHand.Should().Be(true);
            result.EvaluatedHand.HandType.Should().Be(HandType.HighCard);
            result.EvaluatedHand.Value.Should().Be((int) CardRankType.Ace + (int) CardRankType.King +
                                                   (int) CardRankType.Eight + (int) CardRankType.Seven +
                                                   (int) CardRankType.Four);
            result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
        }
        #endregion

        #region OnePair
        // ONE PAIR
        // One pair is a hand that contains two cards of one rank and three cards of three other ranks
        [Fact]
         public void OnePair_NoJoker_ReturnsTrue()
         {
             var onePair = new OnePair();
             
             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card1, card7, card6, card5};

             // Act
             var result = onePair.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.OnePair);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Four * 2 * 5 + (int)CardRankType.Ace + (int)CardRankType.King + (int)CardRankType.Queen);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void OnePair_NoJoker_ReturnsFalse_IfNoPairs()
         {
             var onePair = new OnePair();
             
             var card1 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             // Act
             var result = onePair.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void OnePair_WithJoker_ReturnsTrue()
         {
             var onePair = new OnePair();
             
             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card7, card1, card6, card5, card3};

             // Act
             var result = onePair.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.OnePair);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Ace * 2 * 5 + (int)CardRankType.King + (int)CardRankType.Queen + (int)CardRankType.Seven);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         #endregion

        #region TwoPairs 
         // TWO PAIRS
         // Two pair is a hand that contains two cards of one rank, two cards of another rank and one card of a third rank
         
         [Fact]
         public void TwoPairs_NoJoker_ReturnsTrue()
         {
             // Arrange
             var twoPairs = new TwoPairs();

             var card1 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card2, card4, card5, card7};

             var isJokerGame = false;
             
             // Act
             var result = twoPairs.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.TwoPairs);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Jack * 2 + (int)CardRankType.Three * 2) * 17 + (int) CardRankType.Ace);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void TwoPairs_NoJoker_ReturnsTrue2()
         {
             // Arrange
             var twoPairs = new TwoPairs();

             var card1 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card4, card2, card1};

             var isJokerGame = false;
             
             // Act
             var result = twoPairs.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.TwoPairs);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce * 2 + (int)CardRankType.Seven * 2) * 17 + (int)CardRankType.Ten);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void TwoPairs_NoJoker_ReturnsFalse_IfNoTwoPairs()
         {
             // Arrange
             var twoPairs = new TwoPairs();

             var card1 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;
             
             // Act
             var result = twoPairs.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void TwoPairs_WithJoker_ReturnsTrue()
         {
             // Arrange
             var twoPairs = new TwoPairs();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card7, card4, card5, card6};

             var isJokerGame = true;
             
             // Act
             var result = twoPairs.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.TwoPairs);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ace * 2 + (int)CardRankType.Three * 2) * 17);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         #endregion

        #region ThreeOfAKind
        // THREE OF A KIND
        // Three of a kind is a hand that contains three cards of one rank and two cards of two other ranks
         
         [Fact]
         public void ThreeOfAKind_NoJoker_ReturnsTrue()
         {
             // Arrange
             var threeOfAKind = new ThreeOfAKind();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card1, card2, card7, card6};
             
             var isJokerGame = false;

             // Act
             var result = threeOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.ThreeOfAKind);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Four * 3 * 170);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void ThreeOfAKind_NoJoker_ReturnsFalse()
         {
             // Arrange
             var threeOfAKind = new ThreeOfAKind();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = threeOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void ThreeOfAKind_WithOneJoker_ReturnsTrue()
         {
             // Arrange
             var threeOfAKind = new ThreeOfAKind();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card1, card2, card7, card6};
             
             var isJokerGame = true;

             // Act
             var result = threeOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.ThreeOfAKind);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Four * 3 * 170);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void ThreeOfAKind_WithOneJoker_AndNoPairs_ReturnsFalse()
         {
             // Arrange
             var threeOfAKind = new ThreeOfAKind();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card1, card2, card7, card6};
             
             var isJokerGame = true;

             // Act
             var result = threeOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void ThreeOfAKind_WithTwoJokers_ReturnsTrue()
         {
             // Arrange
             var threeOfAKind = new ThreeOfAKind();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card7, card2, card6, card5};
             
             var isJokerGame = true;

             // Act
             var result = threeOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.ThreeOfAKind);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Ace * 3 * 170);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         #endregion

        #region Straight
         // STRAIGHT
         // A straight is a hand that contains five cards of sequential rank, not all of the same suit
         
         [Fact]
         public void Straight_NoJoker_ReturnsTrue_FiveStraightCards_AceIsMax()
         {
             // Arrange
             var straight = new Straight();
         
             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card6, card4, card3, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_NoJoker_ReturnsTrue_FiveStraightCards_AceIsMin()
         {
             // Arrange
             var straight = new Straight();
         
             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card2, card3, card4, card5, card1};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_NoJoker_ReturnsTrue_FiveStraightCards_NoAce()
         {
             // Arrange
             var straight = new Straight();
         
             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card4 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card5, card4,card3, card2, card1};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_NoJoker_ReturnsTrue_SixStraightCards_NoAce()
         {
             // Arrange
             var straight = new Straight();
         
             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card4 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             //result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six + (int)CardRankType.Seven) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsTrue_FiveStraightCards_AceIsMax()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card1, card5, card4, card3};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsTrue_FiveStraightCards_NoAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card5, card4, card3, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsTrue_FourStraightCards_NoAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card2, card3, card4, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six + (int)CardRankType.Seven + (int)CardRankType.Eight) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsTrue_FourStraightCards_WithAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card5, card3, card6, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace + (int)CardRankType.Ten) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsFalse_IfThereAreNoFourDifferentRanks()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card5, card3, card6, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsTrue_IfThereAreTwoByTwoStraight_NoAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card3, card2, card1, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_OneJoker_ReturnsTrue_IfThereAreThreeAndOneCard_NoAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card4, card3, card2, card1, card7};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereIsThreeStraightCards_MaxIsNotAceOrKing()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card3, card2, card1};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereIsThreeStraightCards_MaxIsKing()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card1, card5, card2};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereIsThreeStraightCards_MaxIsAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card2, card1, card5};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereIsFourStraightCards_MaxIsNotAceOrKing()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card1, card3, card4};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six + (int)CardRankType.Seven) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereIsFourStraightCards_MaxIsKing()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = true;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereIsFourStraightCards_MaxIsAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = true;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereAreFiveStraightCards_MaxIsNotAceOrKing()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card1, card2, card3};
             
             var isJokerGame = false;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six + (int)CardRankType.Seven + (int)CardRankType.Eight) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereAreFiveStraightCards_MaxIsKing()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = true;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Straight_TwoJokers_ReturnsTrue_IfThereAreFiveStraightCards_MaxIsAce()
         {
             // Arrange
             var straight = new Straight();
             
             var card1 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Heart};
             var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card5, card4, card3, card2, card1};
             
             var isJokerGame = true;
         
             // Act
             var result = straight.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Straight);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         #endregion

        #region Flush
         // FLUSH
         // A flush is a hand that contains five cards all of the same suit, not all of sequential rank
         
         [Fact]
         public void Flush_NoJoker_ReturnsTrue_FiveCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card1, card6, card5, card7, card3};
             
             var isJokerGame = false;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ace + (int)CardRankType.Six + (int)CardRankType.Eight + (int)CardRankType.King + (int)CardRankType.Seven) * 1300);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Flush_NoJoker_ReturnsTrue_SixCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card7, card6, card5, card3, card2};
             
             var isJokerGame = false;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Six + (int)CardRankType.Seven + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 1300);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Flush_NoJoker_ReturnsTrue_SevenCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card7, card6, card5, card4, card3};
             
             var isJokerGame = false;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 1300);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void Flush_NoJoker_ReturnsFalse_FourCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void Flush_OneJoker_ReturnsTrue_IfThereAreFourCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.King * 2 + (int)CardRankType.Queen * 3) * 1300);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Suit == CardSuitType.Club || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void Flush_OneJoker_ReturnsTrue_IfThereAreFiveCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.King * 2 + (int)CardRankType.Queen * 2 + (int)CardRankType.Six) * 1300);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Suit == CardSuitType.Club || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void Flush_OneJoker_ReturnsTrue_IfThereAreSixCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.King * 2 + (int)CardRankType.Queen * 3) * 1300);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Suit == CardSuitType.Club || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void Flush_OneJoker_ReturnsFalse_IfThereAreThreeCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void Flush_TwoJokers_ReturnsTrue_IfThereAreThreeCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Diamond};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.King * 3 + (int)CardRankType.Queen + (int)CardRankType.Six) * 1300);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Suit == CardSuitType.Club || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void Flush_TwoJokers_ReturnsTrue_IfThereAreFourCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.King * 3 + (int)CardRankType.Queen + (int)CardRankType.Eight) * 1300);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Suit == CardSuitType.Club || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void Flush_TwoJokers_ReturnsTrue_IfThereAreFiveCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.Flush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.King * 3 + (int)CardRankType.Queen + (int)CardRankType.Eight) * 1300);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Suit == CardSuitType.Club || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void Flush_TwoJokers_ReturnsFalse_IfThereAreTwoCardsOfSameSuit()
         {
             // Arrange
             var flush = new Flush();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
             
             var isJokerGame = true;

             // Act
             var result = flush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         #endregion

        #region FullHouse
         // FULL HOUSE
         // A full house is a hand that contains three cards of one rank and two cards of another rank
         
         [Fact]
         public void FullHouse_NoJoker_ReturnsFalse_IfNoThreeCards()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FullHouse_NoJoker_ReturnsFalse_IfNoTwoCards()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FullHouse_NoJoker_ReturnsFalse_IfAllCardsAreDifferent()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FullHouse_NoJoker_ReturnsTrue()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = false;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FullHouse);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ace * 3 + (int)CardRankType.Six * 2) * 6700);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
         }
         
         [Fact]
         public void FullHouse_NoJoker_ReturnsTrue_IfThereAreTwoByThreeCards()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = false;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FullHouse);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ace * 3 + (int)CardRankType.Six * 2) * 6700);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
         }
         
         [Fact]
         public void FullHouse_OneJoker_ReturnsTrue_IfThereIsThreeOfAKind()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = true;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FullHouse);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Six * 3 + (int)CardRankType.Ace * 2) * 6700);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FullHouse_OneJoker_ReturnsTrue_IfThereIsTwoPairs()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = true;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FullHouse);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Six * 3 + (int)CardRankType.Deuce * 2) * 6700);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Rank == CardRankType.Deuce || x.Rank == CardRankType.Six || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FullHouse_OneJoker_ReturnsFalse_IfThereIsNoTwoPairs()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = true;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FullHouse_TwoJokers_ReturnsTrue_IfThereIsThreeOfAKind()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = true;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FullHouse);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Six * 3 + (int)CardRankType.Three * 2) * 6700);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Rank == CardRankType.Three || x.Rank == CardRankType.Six || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FullHouse_TwoJokers_ReturnsTrue_IfThereIsOnepair()
         {
             // Arrange
             var fullHouse = new FullHouse();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};

             var isJokerGame = true;

             // Act
             var result = fullHouse.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FullHouse);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Six * 2 + (int)CardRankType.Jack * 3) * 6700);
             result.EvaluatedHand.Cards.Should().OnlyContain(x => x.Rank == CardRankType.Jack || x.Rank == CardRankType.Six || x.Rank == CardRankType.Joker);
         }
         #endregion

        #region FourOfAKind
         // FOUR OF A KIND
         // Four of a kind is a hand that contains four cards of one rank and one card of another rank

         [Fact]
         public void FourOfAKind_NoJoker_ReturnsTrue()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FourOfAKind);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ace * 4 + (int)CardRankType.King) * 60000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.King);
         }
         
         [Fact]
         public void FourOfAKind_NoJoker_ReturnsFalse_IfThereAreNoFourOfSameRank()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
             var card3 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FourOfAKind_OneJoker_ReturnsTrue_IFThereIsThreeOfAKind()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = true;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FourOfAKind);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Ace * 4 + (int)CardRankType.King) * 60000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.King || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FourOfAKind_OneJoker_ReturnsFalse_IFThereIsNoThreeOfAKind()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = true;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FourOfAKind_TwoJokers_ReturnsTrue_IFThereIsOnePair()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card3 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = true;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FourOfAKind);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce * 4 + (int)CardRankType.King) * 60000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Deuce || x.Rank == CardRankType.King || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FourOfAKind_TwoJokers_ReturnsTrue_IFThereIsThreeOfAKind()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card3 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = true;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FourOfAKind);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Deuce * 4 + (int)CardRankType.King) * 60000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Deuce || x.Rank == CardRankType.King || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FourOfAKind_TwoJokers_ReturnsFalse_IFThereIsNoOnePair()
         {
             // Arrange
             var fourOfAKind = new FourOfAKind();

             var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card2 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card3 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
             var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
             var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = true;

             // Act
             var result = fourOfAKind.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         #endregion

        #region StraightFlush
         // STRAIGHT FLUSH
         // A straight flush is a hand that contains five cards of sequential rank, all of the same suit
         
         [Fact]
         public void StraightFlush_NoJoker_ReturnsTrue()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card7, card5, card4, card3, card2};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.StraightFlush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void StraightFlush_NoJoker_ReturnsTrue_If6SequentialRanks()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card5, card4, card3};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.StraightFlush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen) * 180000);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void StraightFlush_NoJoker_ReturnsTrue_If7SequentialRanks()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card7, card5, card4, card3};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.StraightFlush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen) * 180000);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void StraightFlush_NoJoker_ReturnsFalse_IfNo5SequentialRanks()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void StraightFlush_NoJoker_ReturnsFalse_IfNo5SequentialCardsWithSameSuit()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }

         [Fact]
         public void StraightFlush_OneJoker_ReturnsTrue_IfThereAreFiveStraightCardsOfSameSuit()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.StraightFlush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void StraightFlush_OneJoker_ReturnsTrue_IfThereAreFiveStraightCards_AndOneOfDifferentSuit()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Spade};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.StraightFlush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
         
         [Fact]
         public void StraightFlush_OneJoker_ReturnsFalse_IfThereAreFiveStraightCards_AndTwoOfDifferentSuit()
         {
             // Arrange
             var straightFlush = new StraightFlush();

             var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
             var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             
             var playerHand = new List<Card> {card1, card2};

             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card6, card5, card4, card3, card2};
             
             var isJokerGame = false;

             // Act
             var result = straightFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.StraightFlush);
             result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
             result.EvaluatedHand.Cards.Should().ContainInOrder(expectedResult);
         }
        #endregion

        #region RoyalFlash
        // TODO: Not tested yet
        // ROYAL FLASH
        // Royal flash is a hand that contains five cards of one rank
        [Fact]
        public void RoyalFlush_WithJoker_ReturnsTrue()
        {
            // Arrange
            var royalFlush = new RoyalFlush();
         
            var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
            var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
             
            var playerHand = new List<Card> {card1, card2};
         
            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
            var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
            var isJokerGame = true; 
         
            // Act
            var result = royalFlush.Check(playerHand, tableCards);
             
            // Assert
            result.IsWinningHand.Should().Be(true);
            result.EvaluatedHand.HandType.Should().Be(HandType.RoyalFlush);
            result.EvaluatedHand.Value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
            result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Ace);
        }
        #endregion
        
        #region FiveOfAKind
         // FIVE OF A KIND
         // Five of a kind is a hand that contains five cards of one rank
         
         [Fact]
         public void FiveOfAKind_NoJokers_ReturnsFalse()
         {
             // Arrange
             var royalFlush = new FiveOfAKind();
         
             var card1 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Heart};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
             var isJokerGame = true; 
         
             // Act
             var result = royalFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FiveOfAKind_OneJoker_ReturnsTrue_IfFourOfAKindAndOneJoker()
         {
             // Arrange
             var royalFlush = new FiveOfAKind();
         
             var card1 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
             var isJokerGame = true; 
         
             // Act
             var result = royalFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FiveOfAKind);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Seven * 5 * 300000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FiveOfAKind_OneJoker_ReturnsFalse_IfThereIsNoFourOfAKind()
         {
             // Arrange
             var royalFlush = new FiveOfAKind();
         
             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
             var isJokerGame = true; 
         
             // Act
             var result = royalFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         
         [Fact]
         public void FiveOfAKind_TwoJokers_ReturnsTrue_IfThereAreThreeOfAKindAndOneJoker()
         {
             // Arrange
             var royalFlush = new FiveOfAKind();
         
             var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
             var isJokerGame = true; 
         
             // Act
             var result = royalFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FiveOfAKind);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Seven * 5 * 300000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Seven || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FiveOfAKind_TwoJokers_ReturnsTrue_IfThereAreFourOfAKindAndOneJoker()
         {
             // Arrange
             var royalFlush = new FiveOfAKind();
         
             var card1 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
             var isJokerGame = true; 
         
             // Act
             var result = royalFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(true);
             result.EvaluatedHand.HandType.Should().Be(HandType.FiveOfAKind);
             result.EvaluatedHand.Value.Should().Be((int)CardRankType.Seven * 5 * 300000);
             result.EvaluatedHand.Cards.Should().Contain(x => x.Rank == CardRankType.Seven || x.Rank == CardRankType.Joker);
         }
         
         [Fact]
         public void FiveOfAKind_TwoJokers_ReturnsFalse_IfThereIsNoThreeOfAKind()
         {
             // Arrange
             var royalFlush = new FiveOfAKind();
         
             var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
             var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
             var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
             var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Heart};
             var card5 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade};
             var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
             var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
             
             var playerHand = new List<Card> {card1, card2};
         
             var tableCards = new List<Card> {card3, card4, card5, card6, card7};
             
             var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
             
             var isJokerGame = true; 
         
             // Act
             var result = royalFlush.Check(playerHand, tableCards);
             
             // Assert
             result.IsWinningHand.Should().Be(false);
             result.EvaluatedHand.HandType.Should().Be(HandType.None);
             result.EvaluatedHand.Value.Should().Be(0);
             result.EvaluatedHand.Cards.Should().BeNull();
         }
         #endregion
    }
}