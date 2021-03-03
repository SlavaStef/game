using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PokerHand.BusinessLogic.Tests
{
    public class CardEvaluationServiceTests
    {
        [Fact]
        public void GetSidePotsWithWinners()
        {
            // // Arrange
            // var sut = new CardEvaluationService(new Mock<ILogger<CardEvaluationService>>().Object);
            //
            // var sidePots = JsonSerializer.Deserialize<List<SidePot>>();
            // var players = JsonSerializer.Deserialize<List<Player>>();
            //
            // // Act
            // var result = sut.GetSidePotsWithWinners(sidePots, players);
            //
            // // Assert
            //
            // result.Should().NotBeNull();
        }
    }
}