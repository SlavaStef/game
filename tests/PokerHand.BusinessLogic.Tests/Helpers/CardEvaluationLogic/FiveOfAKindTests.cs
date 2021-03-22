using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class FiveOfAKindTests
    {
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

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            var expectedResult = new List<Card> {card4, card5, card1, card2, card7};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FiveOfAKind);
            result.Hand.Value.Should().Be((int) CardRankType.Seven * 5 * 300000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[4].SubstitutedCard.Rank.Should().Be(CardRankType.Seven);
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

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void FiveOfAKind_TwoJokers_ReturnsTrue_IfThereAreThreeOfAKind()
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

            var expectedResult = new List<Card> {card4, card5, card2, card6, card7};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FiveOfAKind);
            result.Hand.Value.Should().Be((int) CardRankType.Seven * 5 * 300000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Seven);
            result.Hand.Cards[4].SubstitutedCard.Rank.Should().Be(CardRankType.Seven);
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

            var expectedResult = new List<Card> {card4, card5, card1, card2, card6};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FiveOfAKind);
            result.Hand.Value.Should().Be((int) CardRankType.Seven * 5 * 300000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[4].SubstitutedCard.Rank.Should().Be(CardRankType.Seven);
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

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }
    }
}