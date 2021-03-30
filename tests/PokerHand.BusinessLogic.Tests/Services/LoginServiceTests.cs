using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.DataAccess.Interfaces;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Services
{
    public class LoginServiceTests
    {
        private readonly ILoginService _sut;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>();
        private readonly Mock<IMapper> _mapperMock = new Mock<IMapper>();
        private readonly Mock<ILogger<LoginService>> _loggerMock = new Mock<ILogger<LoginService>>();
        
        public LoginServiceTests()
        {
            _sut = new LoginService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task TryAuthenticate_ReturnsFalseResult_IfLoginNotFound()
        {
            const string providerKey = "providerKey";

            _unitOfWorkMock
                .Setup(x => x.ExternalLogins.GetByProviderKey(providerKey))
                .ReturnsAsync(Guid.Empty);

            var expected = new ResultModel<PlayerProfileDto> {IsSuccess = false, Message = null};

            var result = await _sut.TryAuthenticate(providerKey);
            
            result.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public async Task TryAuthenticate_ReturnsFalse_IfLoginExists()
        {
            const string providerKey = "providerKey";
            var playerId = Guid.NewGuid();

            _unitOfWorkMock
                .Setup(x => x.ExternalLogins.GetByProviderKey(providerKey))
                .ReturnsAsync(playerId);

            _unitOfWorkMock
                .Setup(x => x.Players.GetPlayerAsync(playerId))
                .ReturnsAsync((Player) null);

            var expected = new ResultModel<PlayerProfileDto> {IsSuccess = false, Message = "Player not found"};

            var result = await _sut.TryAuthenticate(providerKey);
            
            result.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public async Task TryAuthenticate_ReturnsPlayerProfile_IfLoginExistsAndPlayerExists()
        {
            const string providerKey = "providerKey";
            var playerId = Guid.NewGuid();
            var player = new Player {Id = playerId};

            _unitOfWorkMock
                .Setup(x => x.ExternalLogins.GetByProviderKey(providerKey))
                .ReturnsAsync(playerId);

            _unitOfWorkMock
                .Setup(x => x.Players.GetPlayerAsync(playerId))
                .ReturnsAsync(player);
            
            _mapperMock
                .Setup(x => x.Map<PlayerProfileDto>(player))
                .Returns(new PlayerProfileDto {Id = playerId});

            var result = await _sut.TryAuthenticate(providerKey);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().BeNullOrEmpty();
            result.Value.Id.Should().Be(playerId);
        }
        
        [Fact]
        public async Task CreateExternalLogin_DoesntCallAddExternalLogin_IfPlayerIsNull()
        {
            var playerId = Guid.NewGuid();

            _unitOfWorkMock
                .Setup(x => x.Players.GetPlayerAsync(playerId))
                .ReturnsAsync((Player)null);

            await _sut.CreateExternalLogin(playerId, ExternalProviderName.Facebook, "");
            
            _unitOfWorkMock.Verify(x => x.Players.GetPlayerAsync(It.IsAny<Guid>()), Times.Once);
            _loggerMock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _unitOfWorkMock
                .Verify(x => x.ExternalLogins
                    .Add(It.IsAny<Player>(), It.IsAny<ExternalProviderName>(), It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task CreateExternalLogin_CallsAddExternalLogin_IfPlayerExists()
        {
            var playerId = Guid.NewGuid();
            const ExternalProviderName providerName = ExternalProviderName.Google;
            const string providerKey = "ProviderKey";
            var player = new Player {Id = playerId};

            _unitOfWorkMock
                .Setup(x => x.Players.GetPlayerAsync(playerId))
                .ReturnsAsync(player);

            _unitOfWorkMock
                .Setup(x => x.ExternalLogins.Add(player, providerName, providerKey));

            await _sut.CreateExternalLogin(playerId, providerName, providerKey);
            
            _unitOfWorkMock.Verify(x => x.Players.GetPlayerAsync(It.IsAny<Guid>()), Times.Once);
            _loggerMock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _unitOfWorkMock
                .Verify(x => x.ExternalLogins
                    .Add(player, providerName, providerKey), Times.Once);
        }
        
        [Fact]
        public async Task DeleteExternalLogin_CallsRemoveByPlayerId()
        {
            var playerId = Guid.NewGuid();

            _unitOfWorkMock
                .Setup(x => x.ExternalLogins.RemoveByPlayerId(playerId));

            await _sut.DeleteExternalLogin(playerId);

            _unitOfWorkMock.Verify(x => x.ExternalLogins.RemoveByPlayerId(playerId), Times.Once);
        }
    }
}