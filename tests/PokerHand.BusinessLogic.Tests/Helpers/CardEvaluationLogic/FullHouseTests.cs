using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class FullHouseTests
    {
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

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FullHouse);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 3 + (int) CardRankType.Six * 2) * 6700);
            result.Hand.Cards.Should()
                .OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
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

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FullHouse);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 3 + (int) CardRankType.Six * 2) * 6700);
            result.Hand.Cards.Should()
                .OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
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
            var card6 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart};
            var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var expectedResult = new List<Card> {card4, card6, card1, card5, card3};

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FullHouse);
            result.Hand.Value.Should().Be(((int) CardRankType.Six * 3 + (int) CardRankType.Ace * 2) * 6700);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[4].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
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

            var expectedResult = new List<Card> {card3, card4, card1, card7, card2};
            
            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FullHouse);
            result.Hand.Value.Should().Be(((int) CardRankType.Six * 3 + (int) CardRankType.Deuce * 2) * 6700);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Six);
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

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void FullHouse_TwoJokers_ReturnsTrue_IfThereIsThreeOfAKind()
        {
            // Arrange
            var fullHouse = new FullHouse();

            var card1 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
            var card3 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
            var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var expectedResult = new List<Card> {card4, card6, card1, card3, card7};

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FullHouse);
            result.Hand.Value.Should().Be(((int) CardRankType.Six * 3 + (int) CardRankType.Ace * 2) * 6700);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
            result.Hand.Cards[4].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
        }

        [Fact]
        public void FullHouse_TwoJokers_ReturnsTrue_IfThereIsOnePair()
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
            
            var expectedResult = new List<Card> {card3, card7, card6, card4, card1};

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FullHouse);
            result.Hand.Value.Should().Be(((int) CardRankType.Six * 2 + (int) CardRankType.Jack * 3) * 6700);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Jack);
            result.Hand.Cards[1].SubstitutedCard.Rank.Should().Be(CardRankType.Jack);
        }
    }
}