using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class FourOfAKindTests
    {
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

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FourOfAKind);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 4 + (int) CardRankType.King) * 60000);
            result.Hand.Cards.Should().Contain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.King);
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

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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
            var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
            var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var expectedResult = new List<Card> {card3, card5, card1, card2, card6};

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FourOfAKind);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace * 4 + (int) CardRankType.King) * 60000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
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

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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
            var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
            var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card3, card5, card1, card2, card6};

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FourOfAKind);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce * 4 + (int) CardRankType.King) * 60000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[2].SubstitutedCard.Rank.Should().Be(CardRankType.Deuce);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Deuce);
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
            var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
            var card7 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var expectedResult = new List<Card> {card3, card5, card1, card2, card6};

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.FourOfAKind);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce * 4 + (int) CardRankType.King) * 60000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[2].SubstitutedCard.Rank.Should().Be(CardRankType.Deuce);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Deuce);
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

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }
    }
}