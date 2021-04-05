using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class TwoPairsTests
    {
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

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.TwoPairs);
            result.Hand.Value.Should().Be(((int) CardRankType.Jack * 2 + (int) CardRankType.Three * 2) * 17 +
                                          (int) CardRankType.Ace);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.TwoPairs);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce * 2 + (int) CardRankType.Seven * 2) * 17 +
                                          (int) CardRankType.Ten);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void TwoPairs_WithJoker_ReturnsTrue_IfThereIsOnePair()
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

            var expectedResult = new List<Card> {card7, card1, card4, card5, card6};

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.TwoPairs);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 2 + (int) CardRankType.Three * 2) * 17 +
                                          (int) CardRankType.King);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void TwoPairs_WithJoker_ReturnsTrueWithJoker_IfThereIsTwoPairs()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
            var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card7, card1, card6, card2, card3};

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.TwoPairs);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 2 + (int) CardRankType.Jack * 2) * 17 +
                                          (int) CardRankType.Seven);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void TwoPairs_WithJoker_ReturnsTrueWithoutJoker_IfThereIsTwoPairs()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Spade};
            var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card6, card7, card5, card2, card3};

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.TwoPairs);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 2 + (int) CardRankType.Jack * 2) * 17 +
                                          (int) CardRankType.Seven);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void TwoPairs_WithJoker_ReturnsFalse_IfThereIsNoPair()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade};
            var card6 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Spade};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            // Act
            var result = twoPairs.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }
    }
}