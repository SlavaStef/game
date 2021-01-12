using System;
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
        [Fact]
        public void HighCart_Check_NoJoker_ReturnsTrue()
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
        
        [Fact]
        public void OnePair_Check_NoJoker_ReturnsTrue()
        {
            // Arrange
            var highCard = new OnePair();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = false;
            
            // Act
            var result = highCard.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.OnePair);
            value.Should().Be((int)CardRankType.Seven * 2 * 5);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Seven);
        }
        
        [Fact]
        public void OnePair_Check_WithJoker_ReturnsTrue()
        {
            // Arrange
            var highCard = new OnePair();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Five, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = true;
            
            // Act
            var result = highCard.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.OnePair);
            value.Should().Be((int)CardRankType.King * 2 * 5);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.King);
        }
        
        [Fact]
        public void OnePair_Check_WithJoker_ReturnsFalse()
        {
            // Arrange
            var highCard = new OnePair();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = true;
            
            // Act
            var result = highCard.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().Be(HandType.None);
            value.Should().Be(0);
        }
        
        [Fact]
        public void TwoPairs_Check_NoJoker_ReturnsTrue()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var playerHand = new List<Card>();
            var firstCard = new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond};
            var secondCard = new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart};
            playerHand.Add(firstCard);
            playerHand.Add(secondCard);
            
            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club}
            };
            var isJokerGame = false;

            var winCards = new List<Card>
            {
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club}
            };
            
            // Act
            var result = twoPairs.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.TwoPairs);
            value.Should().Be(((int)CardRankType.King * 2 + (int)CardRankType.Three * 2) * 17);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.King || x.Rank == CardRankType.Three);
        }
        
        [Fact]
        public void TwoPairs_Check_WithJoker_ReturnsTrue()
        {
            // Arrange
            var twoPairs = new TwoPairs();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = true;

            // Act
            var result = twoPairs.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.TwoPairs);
            value.Should().Be(((int)CardRankType.Ace * 2 + (int)CardRankType.Three * 2) * 17);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Three);
        }
        
        [Fact]
        public void ThreeOfAKind_Check_NoJoker_ReturnsTrue()
        {
            // Arrange
            var threeOfAKind = new ThreeOfAKind();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = false;

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.ThreeOfAKind);
            value.Should().Be((int)CardRankType.Deuce * 3 * 170);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Deuce);
        }
        
        [Fact]
        public void ThreeOfAKind_Check_WithOneJoker_ReturnsTrue()
        {
            // Arrange
            var threeOfAKind = new ThreeOfAKind();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = true;

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.ThreeOfAKind);
            value.Should().Be((int)CardRankType.King * 3 * 170);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.King);
        }
        
        [Fact]
        public void ThreeOfAKind_Check_WithTwoJoker_ReturnsTrue()
        {
            // Arrange
            var threeOfAKind = new ThreeOfAKind();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Red},
                new Card {Rank = CardRankType.Three, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black},
                new Card {Rank = CardRankType.Four, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Deuce, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Five, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = true;

            // Act
            var result = threeOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.ThreeOfAKind);
            value.Should().Be((int)CardRankType.Five * 3 * 170);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Five);
        }
        
        // Straight
        [Fact]
        public void Straight_NoJoker_ReturnsTrue()
        {
            // Arrange
            var straight = new Straight();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Five, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.King, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = false;

            // Act
            var result = straight.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.Straight);
            value.Should().Be(((int)CardRankType.Five + (int)CardRankType.Six + (int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine) * 400);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Five || x.Rank == CardRankType.Six || x.Rank == CardRankType.Seven || x.Rank == CardRankType.Eight || x.Rank == CardRankType.Nine );
        }
        
        // Flush
        [Fact]
        public void Flush_NoJoker_ReturnsTrue()
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
            value.Should().Be(((int)CardRankType.Six + (int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.King + (int)CardRankType.Ace) * 1300);
            winnerCards.Should().OnlyContain(x => x.Suit == CardSuitType.Club);
        }
        
        // Full House
        [Fact]
        public void FullHouse_NoJoker_ReturnsFalse()
        {
            // Arrange
            var fullHouse = new FullHouse();

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
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(false);
            handType.Should().NotBe(HandType.FullHouse);
            value.Should().Be(0);
            winnerCards.Should().BeEmpty();
        }
        
        [Fact]
        public void FullHouse_NoJoker_ReturnsTrue()
        {
            // Arrange
            var fullHouse = new FullHouse();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = false;

            // Act
            var result = fullHouse.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.FullHouse);
            value.Should().Be(((int)CardRankType.Ace * 2 + (int)CardRankType.Six * 3) * 6700);
            winnerCards.Should().OnlyContain(x => x.Rank == CardRankType.Ace || x.Rank == CardRankType.Six);
        }
        
        // Four of a kind
        [Fact]
        public void FourOfAKind_NoJoker_ReturnsTrue()
        {
            // Arrange
            var fourOfAKind = new FourOfAKind();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Spade},
                new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond}
            };
            
            var isJokerGame = false;

            // Act
            var result = fourOfAKind.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.FourOfAKind);
            value.Should().Be((int)CardRankType.Ace * 4 * 60000);
            //winnerCards.Should().Contain(x => x.Rank == CardRankType.Ace);
        }
        
        // Straight Flush
        [Fact]
        public void StraightFlush_NoJoker_ReturnsTrue()
        {
            // Arrange
            var straightFlush = new StraightFlush();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Seven, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Eight, Suit = CardSuitType.Club}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Jack, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Queen, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Nine, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = false;

            // Act
            var result = straightFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.StraightFlush);
            value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
            //winnerCards.Should().Contain(x => x.Rank == CardRankType.Ace);
        }
        
        // Royal Flush
        [Fact]
        public void RoyalFlush_NoJoker_ReturnsTrue()
        {
            // Arrange
            var royalFlush = new RoyalFlush();

            var playerHand = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Club},
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Spade}
            };

            var tableCards = new List<Card>
            {
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Diamond},
                new Card {Rank = CardRankType.Ace, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Joker, Suit = CardSuitType.Black},
                new Card {Rank = CardRankType.Six, Suit = CardSuitType.Heart},
                new Card {Rank = CardRankType.Ten, Suit = CardSuitType.Club}
            };
            
            var isJokerGame = true; 

            // Act
            var result = royalFlush.Check(playerHand, tableCards, isJokerGame, out int value, out HandType handType,
                out List<Card> winnerCards);
            
            // Assert
            result.Should().Be(true);
            handType.Should().Be(HandType.RoyalFlush);
            //value.Should().Be(((int)CardRankType.Seven + (int)CardRankType.Eight + (int)CardRankType.Nine + (int)CardRankType.Ten + (int)CardRankType.Jack) * 180000);
            //winnerCards.Should().Contain(x => x.Rank == CardRankType.Ace);
        }
    }
}