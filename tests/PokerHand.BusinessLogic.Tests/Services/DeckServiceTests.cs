using System;
using System.Linq;
using FluentAssertions;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.Table;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Services
{
    public class DeckServiceTests
    {
        private readonly IDeckService _sut = new DeckService();
        
        [Fact]
        public void GetNewDeck_ReturnsDeckOf52RandomCards_If_TableTypeIsTexasPoker()
        {
            var result = _sut.GetNewDeck(TableType.TexasPoker);

            result.Cards.Count.Should().Be(52);
        }
        
        [Fact]
        public void GetNewDeck_ReturnsDeckOf20RandomCards_If_TableTypeIsRoyalPoker()
        {
            var result = _sut.GetNewDeck(TableType.RoyalPoker);
            
            result.Cards.Count.Should().Be(20);
        }
        
        [Fact]
        public void GetNewDeck_ReturnsDeckOf54RandomCards_If_TableTypeIsJokerPoker()
        {
            var result = _sut.GetNewDeck(TableType.JokerPoker);

            result.Cards.Count.Should().Be(54);
        }
        
        [Fact]
        public void GetNewDeck_ReturnsDeckWith2Jokers_If_TableTypeIsJokerPoker()
        {
            var result = _sut.GetNewDeck(TableType.JokerPoker);
            var jokers = result.Cards.Where(c => c.Rank is CardRankType.Joker).ToList();

            jokers.Should().HaveCount(2);
            jokers[0].Suit.Should().NotBeEquivalentTo(jokers[1].Suit);
        }
        
        [Fact]
        public void GetNewDeck_ReturnsCardsInRandomOrder()
        {
            var result = _sut.GetNewDeck(TableType.TexasPoker);

            result.Cards.Select(c => c.Rank).Should().NotBeInAscendingOrder();
            result.Cards.Select(c => c.Rank).Should().NotBeInDescendingOrder();
        }
        
        [Fact]
        public void GetNewDeck_ReturnsAllDifferentCards()
        {
            var result = _sut.GetNewDeck(TableType.TexasPoker);
            
            result.Cards.Should().OnlyHaveUniqueItems();
        }
        
        [Fact]
        public void GetNewDeck_ThrowsException_If_TableTypeIsWrong()
        {
            _sut.Invoking(s => s.GetNewDeck((TableType) 10)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetRandomCardsFromDeck_ReturnsCorrectNumberOfCards(int numberOfCards)
        {
            var deck = _sut.GetNewDeck(TableType.TexasPoker);

            var result = _sut.GetRandomCardsFromDeck(deck, numberOfCards);

            result.Count.Should().Be(numberOfCards);
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(60)]
        public void GetRandomCardsFromDeck_ReturnsNull_If_NumberOfCardsIsIncorrect(int numberOfCards)
        {
            var deck = _sut.GetNewDeck(TableType.TexasPoker);

            var result = _sut.GetRandomCardsFromDeck(deck, numberOfCards);

            result.Should().BeNull();
        }
    }
}