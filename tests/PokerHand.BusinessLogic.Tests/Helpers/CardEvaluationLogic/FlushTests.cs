using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class FlushTests
    {
        // A flush is a hand that contains five cards all of the same suit, not all of sequential rank
        // FLUSH

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

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.Six +
                                                    (int) CardRankType.Eight + (int) CardRankType.King +
                                                    (int) CardRankType.Seven) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Six + (int) CardRankType.Seven +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Seven + (int) CardRankType.Eight +
                                                    (int) CardRankType.Queen + (int) CardRankType.King +
                                                    (int) CardRankType.Ace) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
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

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void Flush_OneJoker_ReturnsTrue_IfThereAreFourCardsOfSameSuit_AceIsMax()
        {
            // Arrange
            var flush = new Flush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
            var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card5, card1, card7, card6, card4};

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Jack +
                                                    (int) CardRankType.Four) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[1].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[1].SubstitutedCard.Rank.Should().Be(CardRankType.King);
        }
        
        [Fact]
        public void Flush_OneJoker_ReturnsTrue_IfThereAreFourCardsOfSameSuit_KingIsMax()
        {
            // Arrange
            var flush = new Flush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond};
            var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond};
            var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card6, card7, card4, card5};

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Four +
                                                    (int) CardRankType.Deuce) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
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
            var card5 = new Card {Rank = CardRankType.Five, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card6, card4, card3, card5};
            
            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Six +
                                                    (int) CardRankType.Five) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
        }

        [Fact]
        public void Flush_OneJoker_ReturnsTrue_IfThereAreSixCardsOfSameSuit()
        {
            // Arrange
            var flush = new Flush();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card6, card7, card4, card3};

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Eight +
                                                    (int) CardRankType.Six) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
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

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
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

            var expectedResult = new List<Card> {card1, card6, card4, card2, card3};

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Jack +
                                                    (int) CardRankType.Six) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
            result.Hand.Cards[3].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Jack);
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

            var expectedResult = new List<Card> {card1, card6, card4, card2, card5};

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Jack +
                                                    (int) CardRankType.Eight) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
            result.Hand.Cards[3].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Jack);
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

            var expectedResult = new List<Card> {card1, card6, card4, card2, card5};

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.Flush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Jack +
                                                    (int) CardRankType.Eight) * 1300);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
            result.Hand.Cards[0].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[0].SubstitutedCard.Rank.Should().Be(CardRankType.Ace);
            result.Hand.Cards[3].SubstitutedCard.Suit.Should().Be(CardSuitType.Club);
            result.Hand.Cards[3].SubstitutedCard.Rank.Should().Be(CardRankType.Jack);
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

            // Act
            var result = flush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }
    }
}