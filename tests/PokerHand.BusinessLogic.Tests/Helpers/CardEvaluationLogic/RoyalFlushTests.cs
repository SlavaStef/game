﻿using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class RoyalFlushTests
    {
        // ROYAL FLASH
        // Royal flash is a hand that contains five cards of one rank
        [Fact]
        public void RoyalFlush_NoJoker_ReturnsTrue_IfThereIsStraightFlushWithAce()
        {
            // Arrange
            var royalFlush = new RoyalFlush();

            var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card2 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card2, card6, card7, card5};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.RoyalFlush);
            result.Hand.Value.Should().Be(((int) CardRankType.Ace + (int) CardRankType.King +
                                                    (int) CardRankType.Queen + (int) CardRankType.Jack +
                                                    (int) CardRankType.Ten) * 200000);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void RoyalFlush_NoJoker_ReturnsFalse_IfThereIsStraightFlush_AndNoAce()
        {
            // Arrange
            var royalFlush = new RoyalFlush();

            var card1 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card2 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
            var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card2, card6, card7, card5};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(0);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void RoyalFlush_NoJoker_ReturnsFalse_IfThereIsStraightAndNoFlush()
        {
            // Arrange
            var royalFlush = new RoyalFlush();

            var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card2 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade};
            var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card1, card2, card6, card7, card5};

            // Act
            var result = royalFlush.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(0);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }
    }
}