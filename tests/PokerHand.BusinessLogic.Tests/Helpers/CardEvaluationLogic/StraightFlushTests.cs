using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class StraightFlushTests
    {
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Seven + (int) CardRankType.Eight +
                                                    (int) CardRankType.Nine + (int) CardRankType.Ten +
                                                    (int) CardRankType.Jack) * 180000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Eight + (int) CardRankType.Nine +
                                                    (int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen) * 180000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Eight + (int) CardRankType.Nine +
                                                    (int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen) * 180000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Seven + (int) CardRankType.Eight +
                                                    (int) CardRankType.Nine + (int) CardRankType.Ten +
                                                    (int) CardRankType.Jack) * 180000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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
            var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card6, card5, card4, card3, card2};

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Seven + (int) CardRankType.Eight +
                                                    (int) CardRankType.Nine + (int) CardRankType.Ten +
                                                    (int) CardRankType.Jack) * 180000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Seven + (int) CardRankType.Eight +
                                                    (int) CardRankType.Nine + (int) CardRankType.Ten +
                                                    (int) CardRankType.Jack) * 180000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
    }
}