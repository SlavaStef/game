using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class StraightTests
    {
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce + (int) CardRankType.Three +
                                                    (int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            var expectedResult = new List<Card> {card5, card4, card3, card2, card1};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce + (int) CardRankType.Three +
                                                    (int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Six) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Three + (int) CardRankType.Four +
                                                    (int) CardRankType.Five + (int) CardRankType.Six +
                                                    (int) CardRankType.Seven) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            var expectedResult = new List<Card> {card7, card1, card5, card4, card3};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Ace);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Six + (int) CardRankType.Seven +
                                                    (int) CardRankType.Eight) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Eight);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Jack + (int) CardRankType.Queen +
                                                    (int) CardRankType.King + (int) CardRankType.Ace +
                                                    (int) CardRankType.Ten) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Ten);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            var expectedResult = new List<Card> {card4, card3, card7, card2, card1};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce + (int) CardRankType.Three +
                                                    (int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Six) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Four);
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

            var expectedResult = new List<Card> {card4, card7, card3, card2, card1};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce + (int) CardRankType.Three +
                                                    (int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Six) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Five);
        }

        [Fact]
        public void Straight_TwoJokers_ReturnsTrue_IfThereIsThreeStraightCards_MaxIsNotAceOrKing()
        {
            // Arrange
            var straight = new Straight();

            var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Heart};
            var card2 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
            var card3 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Spade};
            var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Diamond};
            var card6 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black};
            var card7 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};


            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card7, card6, card3, card2, card1};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Deuce + (int) CardRankType.Three +
                                                    (int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Six) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Six);
            result.Hand.Cards[1].SubstitutedCard.Rank.Should().Be(CardRankType.Five);
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

            var expectedResult = new List<Card> {card6, card1, card5, card2, card7};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
            result.Hand.Cards[4].SubstitutedCard.Rank.Should().Be(CardRankType.Ten);
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

            var expectedResult = new List<Card> {card2, card1, card5, card6, card7};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[3].SubstitutedCard.Rank = CardRankType.Jack;
            result.Hand.Cards[4].SubstitutedCard.Rank = CardRankType.Ten;
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

            var expectedResult = new List<Card> {card7, card6, card1, card3, card4};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Three + (int) CardRankType.Four +
                                                    (int) CardRankType.Five + (int) CardRankType.Six +
                                                    (int) CardRankType.Seven) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Ace);
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

            var expectedResult = new List<Card> {card5, card4, card3, card2, card6};

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Ten);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Four + (int) CardRankType.Five +
                                                    (int) CardRankType.Six + (int) CardRankType.Seven +
                                                    (int) CardRankType.Eight) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Eight);
            result.Hand.Cards[1].SubstitutedCard.Rank.Should().Be(CardRankType.Seven);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards.First(c => c.Rank is CardRankType.Joker).SubstitutedCard.Rank.Should()
                .Be(CardRankType.Ace);
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

            // Act
            var result = straight.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Straight);
            result.Hand.Value.Should().Be(((int) CardRankType.Ten + (int) CardRankType.Jack +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 400);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
    }
}