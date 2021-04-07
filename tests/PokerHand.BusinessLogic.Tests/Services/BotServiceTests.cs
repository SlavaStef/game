using System;
using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using Moq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.Table;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Services
{
    public class BotServiceTests
    {
        private IBotService _sut;
        
        private readonly Mock<ICardEvaluationService> _cardEvaluationServiceMock = new();
        private readonly Mock<IRuleSet<Player>> _faker = new();
        private readonly Random _random = new();

        public BotServiceTests()
        {
            _sut = new BotService(_cardEvaluationServiceMock.Object);
        }
        
        [Fact]
        public void Create_ReturnsNewPlayer()
        {
            var table = new Table
            {
                Title = TableTitle.TropicalHouse, 
                Players = new List<Player>
                {
                    new Bot(),
                    new Bot()
                }
            };

            var result = _sut.Create(table, BotComplexity.Hard);

            result.GetType().Should().Be(typeof(Bot));
            result.Complexity.Should().Be(BotComplexity.Hard);
            result.UserName.Should().NotBeNull();
            result.TotalMoney.Should().BeInRange(5_000, 25_000);
            result.StackMoney.Should().BeInRange(500, 5_000);
            result.StackMoney.Should().BeLessOrEqualTo(result.TotalMoney);
            result.CoinsAmount.Should().BeInRange(0, 100);
            result.Experience.Should().BeInRange(50, 500);
            result.GamesPlayed.Should().BeInRange(5, 50);
            result.BiggestWin.Should().BeInRange(1, 10_000_000);
            result.SitAndGoWins.Should().BeInRange(0, 20);
            result.IsAutoTop.Should().BeTrue();
            result.IsReady.Should().BeTrue();
        }

        [Fact]
        public void Act_ReturnsAnyAction()
        {
            var table = new Table
            {
                Title = TableTitle.TropicalHouse, 
                Players = new List<Player>
                {
                    new Player(),
                    new Player()
                }
            };

            var bot = _sut.Create(table, BotComplexity.Hard);
            
            table.Players.Add(bot);
            
            //TODO: finish test
        }
    }
}