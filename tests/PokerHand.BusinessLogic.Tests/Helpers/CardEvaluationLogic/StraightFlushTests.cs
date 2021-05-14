using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class StraightFlushTests
    {
        // STRAIGHT FLUSH
        // A straight flush is a hand that contains five cards of sequential rank, all of the same suit
        
        private const int Rate = 180_000;
        

        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue_IfThereAreFiveSequentialCardsOfSameRank()
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
                                                    (int) CardRankType.Jack) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue_IfThereAreSixSequentialCardsOfSameRank()
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
                                                    (int) CardRankType.Queen) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue_IfThereAreSevenSequentialCardsOfSameRank()
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
                                                    (int) CardRankType.Queen) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void StraightFlush_NoJoker_ReturnsFalse_IfNo5SequentialRanks_AndAllCardsOfSameSuit()
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
        public void StraightFlush_NoJoker_ReturnsFalse_IfThereAreFiveAndMoreSequentialCards_ButNotOfSameSuit()
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

        // One joker
        
        [Fact]
        public void StraightFlush_OneJoker_ReturnsTrue_IfThereAre4StraightCardsOfSameSuit_AndMaxSuitIsNotAce()
        {
            // Arrange
            var straightFlush = new StraightFlush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
            var card3 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card7, card6, card5, card4};

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.King + (int) CardRankType.Queen +
                                           (int) CardRankType.Jack + (int) CardRankType.Ten +
                                           (int) CardRankType.Nine) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void StraightFlush_OneJoker_ReturnsTrue_IfThereAre3StraightAnd1OfSameSuit()
        {
            // Arrange
            var straightFlush = new StraightFlush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
            var card3 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card7, card1, card6, card5, card4};

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Queen + (int) CardRankType.Jack +
                                           (int) CardRankType.Ten + (int) CardRankType.Nine +
                                           (int) CardRankType.Eight) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void StraightFlush_OneJoker_ReturnsTrue_IfThereAre1And3StraightOfSameSuit()
        {
            // Arrange
            var straightFlush = new StraightFlush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
            var card3 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card7, card6, card5, card1, card4};

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Queen + (int) CardRankType.Jack +
                                           (int) CardRankType.Ten + (int) CardRankType.Nine +
                                           (int) CardRankType.Eight) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void StraightFlush_OneJoker_ReturnsTrue_IfThereAre2StraightAnd2StraightOfSameSuit()
        {
            // Arrange
            var straightFlush = new StraightFlush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
            var card3 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card7, card6, card1, card5, card4};

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Queen + (int) CardRankType.Jack +
                                           (int) CardRankType.Ten + (int) CardRankType.Nine +
                                           (int) CardRankType.Eight) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void StraightFlush_OneJoker_ReturnsTrue_IfThereAre5StraightOfSameSuitAndMaxSuitIsNotAce()
        {
            // Arrange
            var royalFlush = new StraightFlush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Spade};
            var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card7, card6, card5, card4};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.King + (int) CardRankType.Queen +
                                           (int) CardRankType.Jack + (int) CardRankType.Ten +
                                           (int) CardRankType.Nine) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void StraightFlush_OneJoker_ReturnsTrue_IfThereAreFiveStraightCardsOfSameSuit_AndJokerIsInsideCombination()
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

            var expectedResult = new List<Card> {card7, card6, card5, card4, card3};

            // Act
            var result = straightFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.StraightFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Queen + (int) CardRankType.Jack +
                                           (int) CardRankType.Ten + (int) CardRankType.Nine + 
                                           (int) CardRankType.Eight) * Rate);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
        
        
    }
}