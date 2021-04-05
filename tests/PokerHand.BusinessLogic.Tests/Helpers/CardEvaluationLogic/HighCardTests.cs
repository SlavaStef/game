using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.Helpers.CardEvaluationLogic;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.CardEvaluation;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.CardEvaluationLogic
{
    public class HighCardTests
    {
        // HIGH CARD
        // High card is a hand that does not fall into any other category
        // High card with Joker is impossible
        
        [Fact]
        public void HighCard_NoJoker_ReturnsTrue()
        {
            // Arrange
            var fullHouse = new HighCard();

            var card1 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card2 = new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart};
            var card3 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
            var card5 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};

            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};

            var expectedResult = new List<Card> {card6, card3, card7, card1, card2};

            // Act
            var result = fullHouse.Check(playerHand, tableCards);

            // Assert
            result.IsWinningHand.Should().Be(true);
            result.Hand.HandType.Should().Be(HandType.HighCard);
            result.Hand.Value.Should().Be((int) CardRankType.Ace + (int) CardRankType.King +
                                          (int) CardRankType.Eight + (int) CardRankType.Seven +
                                          (int) CardRankType.Four);
            result.Hand.Cards.Should().ContainInOrder(expectedResult);
        }
    }
}