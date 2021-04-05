using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using PokerHand.BusinessLogic.Helpers.BotLogic;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Card;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Helpers.BotLogic
{
    public class HardBotLogicTests
    {
        [Theory]
        [InlineData(1000, 0, 100, CardRankType.Seven, CardSuitType.Spade, CardRankType.Five, CardSuitType.Spade)]
        [InlineData(1000, 0, 100, CardRankType.Four, CardSuitType.Diamond, CardRankType.Five, CardSuitType.Spade)]
        public void ActOnFirstRound_ReturnsCall(int stackMoney, int currentBet, int currentMaxBet, CardRankType firstCardRank, 
            CardSuitType firstCardSuit, CardRankType secondCardRank, CardSuitType secondCardSuit)
        {
            // Arrange
            var bot = new Player
            {
                IndexNumber = 1,
                StackMoney = stackMoney,
                CurrentBet = currentBet,
                PocketCards = new List<Card>
                {
                    new() {Rank = firstCardRank, Suit = firstCardSuit},
                    new() {Rank = secondCardRank, Suit = secondCardSuit}
                }
            };
            var table = new Table
            {
                CurrentStage = RoundStageType.WageringPreFlopRound,
                CurrentMaxBet = currentMaxBet
            };

            var evaluationService = Substitute.For<ICardEvaluationService>();
            evaluationService.EvaluatePlayerHand(table.CommunityCards, bot).Returns(bot);

            var random = Substitute.For<Random>();
            var sut = new HardBotLogic(evaluationService, random);

            // Act
            var result = sut.Act(bot, table);

            // Assert
            result.ActionType.Should().Be(PlayerActionType.Call);
            result.PlayerIndexNumber = 1;
        }
        
        [Theory]
        [InlineData(1000, 100, 100, 40, CardRankType.Seven, CardSuitType.Spade, CardRankType.Five, CardSuitType.Spade)]
        public void ActOnFirstRound_ReturnsRaise(int stackMoney, int currentBet, int currentMaxBet, int bigBlind, 
            CardRankType firstCardRank, CardSuitType firstCardSuit, CardRankType secondCardRank, CardSuitType secondCardSuit)
        {
            // Arrange
            var bot = new Player
            {
                IndexNumber = 1,
                StackMoney = stackMoney,
                CurrentBet = currentBet,
                PocketCards = new List<Card>
                {
                    new() {Rank = firstCardRank, Suit = firstCardSuit},
                    new() {Rank = secondCardRank, Suit = secondCardSuit}
                }
            };
            var table = new Table
            {
                CurrentStage = RoundStageType.WageringPreFlopRound,
                CurrentMaxBet = currentMaxBet,
                BigBlind = bigBlind
            };

            var evaluationService = Substitute.For<ICardEvaluationService>();
            evaluationService.EvaluatePlayerHand(table.CommunityCards, bot).Returns(bot);
            
            var random = Substitute.For<Random>();
            random.Next(1, 100).Returns(10);
            var sut = new HardBotLogic(evaluationService, random);

            // Act
            var result = sut.Act(bot, table);

            // Assert
            result.ActionType.Should().Be(PlayerActionType.Raise);
            result.Amount.Should().Be(bigBlind);
            result.PlayerIndexNumber = 1;
        }
        
        [Theory]
        [InlineData(1000, 100, 100, 40, CardRankType.Seven, CardSuitType.Spade, CardRankType.Five, CardSuitType.Spade)]
        public void ActOnFirstRound_ReturnsCheck(int stackMoney, int currentBet, int currentMaxBet, int bigBlind, 
            CardRankType firstCardRank, CardSuitType firstCardSuit, CardRankType secondCardRank, CardSuitType secondCardSuit)
        {
            // Arrange
            var bot = new Player
            {
                IndexNumber = 1,
                StackMoney = stackMoney,
                CurrentBet = currentBet,
                PocketCards = new List<Card>
                {
                    new() {Rank = firstCardRank, Suit = firstCardSuit},
                    new() {Rank = secondCardRank, Suit = secondCardSuit}
                }
            };
            var table = new Table
            {
                CurrentStage = RoundStageType.WageringPreFlopRound,
                CurrentMaxBet = currentMaxBet,
                BigBlind = bigBlind
            };

            var evaluationService = Substitute.For<ICardEvaluationService>();
            evaluationService.EvaluatePlayerHand(table.CommunityCards, bot).Returns(bot);
            
            var random = Substitute.For<Random>();
            random.Next(1, 100).Returns(30);
            var sut = new HardBotLogic(evaluationService, random);

            // Act
            var result = sut.Act(bot, table);

            // Assert
            result.ActionType.Should().Be(PlayerActionType.Check);
            result.PlayerIndexNumber = 1;
        }
    }
}