using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class ThreeOfAKindTests
    {
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

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.ThreeOfAKind);
            result.Hand.Value.Should()
                .Be((int) CardRankType.Four * 3 * 170 + (int) CardRankType.Ace + (int) CardRankType.King);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void ThreeOfAKind_WithOneJoker_ReturnsTrue_IfThereIsAPair()
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

            var expectedResult = new List<Card> {card1, card2, card4, card7, card6};

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.ThreeOfAKind);
            result.Hand.Value.Should()
                .Be((int) CardRankType.Four * 3 * 170 + (int) CardRankType.Ace + (int) CardRankType.King);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            var expectedResult = new List<Card> {card7, card4, card2, card6, card5};

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.ThreeOfAKind);
            result.Hand.Value.Should().Be((int) CardRankType.Ace * 3 * 170 + (int) CardRankType.King + (int) CardRankType.Queen);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
    }
}