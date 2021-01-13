using System.Collections.Generic;
using FluentAssertions;
using PokerHand.BusinessLogic.HandEvaluator.Hands;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Common.Helpers.Card;
using Xunit;

namespace PokerHand.BusinessLogic.Tests
{
    public class HandEvaluatorTests
    {
        // HIGH CARD
        // High card is a hand that does not fall into any other category
        
        [Fact]
        public void HighCart_NoJoker_ReturnsTrue()
        {
            // Arrange
            var highCard = new HighCard();

            var playerHand = new List<Card>();
            var firstCard = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
            var secondCard = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart};
            
            playerHand.Add(firstCard);
            playerHand.Add(secondCard);
            
            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club}
            };
            var isJokerGame = false;
            
            // Act
            var result = highCard.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.HighCard);
            value.Should().Be((int) firstCard.Rank);
            winnerCards.Should().Contain(firstCard);
        }
        
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

            var isJokerGame = false;
            
            // Act
            var result = onePair.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.OnePair);
            value.Should().Be((int)CardRankType.Four * 2 * 5);
            winnerCards.Should().ContainInOrder(expectedResult);
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

            var isJokerGame = false;
            
            // Act
            var result = onePair.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeNull();
        }

        // TWO PAIRS
        // Two pair is a hand that contains two cards of one rank, two cards of another rank and one card of a third rank
        
        [Fact]
        public void TwoPairs_NoJoker_ReturnsTrue()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var card1 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Diamond};
            var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            
            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var expectedResult = new List<Card> {card1, card2, card4, card5, card7};

            var isJokerGame = false;
            
            // Act
            var result = twoPairs.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.TwoPairs);
            value.Should().Be(((int)CardRankType.Jack * 2 + (int)CardRankType.Three * 2) * 17);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void TwoPairs_NoJoker_ReturnsFalse_IfNoTwoPairs()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var card1 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Diamond};
            var card2 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
            var card3 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club};
            var card5 = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Spade};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            
            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var isJokerGame = false;
            
            // Act
            var result = twoPairs.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeNull();
        }
        
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
            
            var isJokerGame = false;

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.ThreeOfAKind);
            value.Should().Be((int)CardRankType.Four * 3 * 170);
            winnerCards.Should().ContainInOrder(expectedResult);
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
            
            var isJokerGame = false;

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeNull();
        }
        
        // STRAIGHT
        // A straight is a hand that contains five cards of sequential rank, not all of the same suit
        
        [Fact]
        public void Straight_NoJoker_ReturnsTrue_5StraightCards_AceIsMax()
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
            
            var expectedResult = new List<Card> {card7, card3, card4, card6, card1};
            
            var isJokerGame = false;

            // Act
            var result = straight.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Straight);
            value.Should().Be(((int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 400);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void Straight_NoJoker_ReturnsTrue_5StraightCards_AceIsMin()
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
            
            var isJokerGame = false;

            // Act
            var result = straight.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Straight);
            value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Ace) * 400);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void Straight_NoJoker_ReturnsTrue_5StraightCards_NoAce()
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
            
            var expectedResult = new List<Card> {card1, card2, card3, card4, card5};
            
            var isJokerGame = false;

            // Act
            var result = straight.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Straight);
            value.Should().Be(((int)CardRankType.Deuce + (int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six) * 400);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void Straight_NoJoker_ReturnsTrue_6StraightCards_NoAce()
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
            
            var expectedResult = new List<Card> {card2, card3, card4, card5, card6};
            
            var isJokerGame = false;

            // Act
            var result = straight.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Straight);
            //value.Should().Be(((int)CardRankType.Three + (int)CardRankType.Four + (int)CardRankType.Five + (int)CardRankType.Six + (int)CardRankType.Seven) * 400);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        // FLUSH
        // A flush is a hand that contains five cards all of the same suit, not all of sequential rank
        
        [Fact]
        public void Flush_NoJoker_ReturnsTrue_5CardsOfSameSuit()
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
            
            var expectedResult = new List<Card> {card3, card7, card5, card6, card1};
            
            var isJokerGame = false;

            // Act
            var result = flush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Flush);
            value.Should().Be(((int)CardRankType.Ace + (int)CardRankType.Six + (int)CardRankType.Eight + (int)CardRankType.King + (int)CardRankType.Seven) * 1300);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void Flush_NoJoker_ReturnsTrue_6CardsOfSameSuit()
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
            
            var expectedResult = new List<Card> {card2, card3, card5, card6, card7};
            
            var isJokerGame = false;

            // Act
            var result = flush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Flush);
            value.Should().Be(((int)CardRankType.Six + (int)CardRankType.Seven + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 1300);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void Flush_NoJoker_ReturnsTrue_7CardsOfSameSuit()
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
            
            var expectedResult = new List<Card> {card3, card4, card5, card6, card7};
            
            var isJokerGame = false;

            // Act
            var result = flush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Flush);
            value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Queen + (int)CardRankType.King + (int)CardRankType.Ace) * 1300);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void Flush_NoJoker_ReturnsFalse_4CardsOfSameSuit()
        {
            // Arrange
            var flush = new Flush();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = false;

            // Act
            var result = flush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Flush);
            value.Should().Be(((int)CardRankType.Ace + (int)CardRankType.Six + (int)CardRankType.Eight + (int)CardRankType.King + (int)CardRankType.Seven) * 1300);
            winnerCards.Should().OnlyContain(x => x.Suit == CardSuitType.Club);
        }
        
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
            
            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeNull();
        }
        
        [Fact]
        public void FullHouse_NoJoker_ReturnsFalse_IfNoTwoCards()
        {
            // Arrange
            var fullHouse = new FullHouse();

            var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card2 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart};
            var card3 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club};
            var card4 = new Card {Rank = CardRankType.Six, Suit = CardSuitType.Spade};
            var card5 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
            var card6 = new Card {Rank = CardRankType.King, Suit = CardSuitType.Club};
            var card7 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
            
            var playerHand = new List<Card> {card1, card2};

            var tableCards = new List<Card> {card3, card4, card5, card6, card7};
            
            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeNull();
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
            
            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeNull();
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

            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.FullHouse);
            value.Should().Be(((int)CardRankType.Ace * 3 + (int)CardRankType.Six * 2) * 6700);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
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

            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.FullHouse);
            value.Should().Be(((int)CardRankType.Ace * 3 + (int)CardRankType.Six * 2) * 6700);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
        }
        
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
            
            var isJokerGame = false;

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.FourOfAKind);
            value.Should().Be((int)CardRankType.Ace * 4 * 60000);
            winnerCards.Should().Contain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.King);
        }
        
        // STRAIGHT FLUSH
        // A straight flush is a hand that contains five cards of sequential rank, all of the same suit
        
        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue()
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
            
            var expectedResult = new List<Card> {card2, card3, card4, card5, card7};
            
            var isJokerGame = false;

            // Act
            var result = straightFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.StraightFlush);
            value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue_If6SequentialRanks()
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
            
            var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
            
            var isJokerGame = false;

            // Act
            var result = straightFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.StraightFlush);
            value.Should().Be(((int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen) * 180000);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue_If7SequentialRanks()
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
            
            var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
            
            var isJokerGame = false;

            // Act
            var result = straightFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.StraightFlush);
            value.Should().Be(((int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack + (int)CardRankType.Queen) * 180000);
            winnerCards.Should().ContainInOrder(expectedResult);
        }
        
        [Fact]
        public void StraightFlush_NoJoker_ReturnsFalse_IfNo5SequentialRanks()
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
            
            var isJokerGame = false;

            // Act
            var result = straightFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeEmpty();
        }
        
        [Fact]
        public void StraightFlush_NoJoker_ReturnsFalse_IfNo5SequentialCardsWithSameSuit()
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
            
            var isJokerGame = false;

            // Act
            var result = straightFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
            winnerCards.Should().BeEmpty();
        }
        
        // TODO: Not tested yet
        // ROYAL FLASH
        // Royal flash is a hand that contains five cards of one rank
        // [Fact]
        // public void RoyalFlush_WithJoker_ReturnsTrue()
        // {
        //     // Arrange
        //     var royalFlush = new RoyalFlush();
        //
        //     var card1 = new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond};
        //     var card2 = new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club};
        //     var card3 = new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club};
        //     var card4 = new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club};
        //     var card5 = new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club};
        //     var card6 = new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Club};
        //     var card7 = new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club};
        //     
        //     var playerHand = new List<Card> {card1, card2};
        //
        //     var tableCards = new List<Card> {card3, card4, card5, card6, card7};
        //     
        //     var expectedResult = new List<Card> {card3, card4, card5, card7, card6};
        //     
        //     var isJokerGame = true; 
        //
        //     // Act
        //     var result = royalFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
        //         out List<Card> winnerCards);
        //     
        //     // Assert
        //     result.Should().Be(true);
        //     handType.Should().Be(HandType.RoyalFlush);
        //     //value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
        //     //winnerCards.Should().Contain(x => x.Rank == CardRankType.Ace);
        // }
    }
}