using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class OnePairTests
    {
        // ONE PAIR
        // One pair is a hand that contains two cards of one rank and three cards of three other ranks
        
        [Fact]
        public void OnePair_NoJoker_ReturnsTrue()
        {
            var onePair = new OnePair();

            var card1 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Diamond};
            var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card4, card1, card7, card6, card5};

            // Act
            var result = onePair.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.OnePair);
            result.Hand.Value.Should().Be((int) CardRankType.Four * 2 * 5 + (int) CardRankType.Ace +
                                          (int) CardRankType.King + (int) CardRankType.Queen);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }

        [Fact]
        public void OnePair_NoJoker_ReturnsFalse_IfNoPairs()
        {
            var onePair = new OnePair();

            var card1 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Diamond};
            var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            // Act
            var result = onePair.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(false);
            result.Hand.HandType.Should().Be(HandType.None);
            result.Hand.Value.Should().Be(0);
            result.Hand.Cards.Should().BeNull();
        }

        [Fact]
        public void OnePair_WithJoker_ReturnsTrue()
        {
            var onePair = new OnePair();

            var card1 = new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red};
            var card2 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card7, card1, card6, card5, card3};

            // Act
            var result = onePair.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.OnePair);
            result.Hand.Value.Should().Be((int) CardRankType.Ace * 2 * 5 + (int) CardRankType.King +
                                          (int) CardRankType.Queen + (int) CardRankType.Seven);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
    }
}