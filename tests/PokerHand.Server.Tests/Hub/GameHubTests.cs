using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Server.Hubs;
using PokerHand.Server.Hubs.Interfaces;
using Xunit;

namespace PokerHand.Server.Tests.Hub
{
    public class GameHubTests
    {
        private readonly GameHub _hub;
        private readonly Mock<IGameHubClient> _clientProxyMock = new();
        private readonly Mock<IHubCallerClients<IGameHubClient>> _clientsMock = new();
        private readonly Mock<HubCallerContext> _clientContextMock = new();
        
        private readonly Mock<ITableService> _tableServiceMock = new();
        private readonly Mock<IPlayerService> _playerServiceMock = new();
        private readonly Mock<ILogger<GameHub>> _loggerMock = new();
        private readonly Mock<IPlayersOnline> _playersOnlineMock = new();
        private readonly Mock<ITablesOnline> _tablesOnlineMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IMediaService> _mediaServiceMock = new();
        private readonly Mock<IGameProcessService> _gameProcessServiceMock = new();

        public GameHubTests()
        {
            _hub = new GameHub(_tableServiceMock.Object, _playerServiceMock.Object, _loggerMock.Object,
                _playersOnlineMock.Object, _tablesOnlineMock.Object, _mapperMock.Object, _mediaServiceMock.Object,
                _gameProcessServiceMock.Object);
        }

        [Fact]
        public async Task RegisterNewPlayer_ReturnsSerializedPlayerDto_ToCaller()
        {
            // Arrange
            _playerServiceMock
                .Setup(x => x.CreatePlayer("playerName"))
                .ReturnsAsync(new PlayerProfileDto {UserName = "playerName"});
            
            _clientsMock
                .Setup(clients => clients.Caller)
                .Returns(_clientProxyMock.Object);

            _clientContextMock
                .Setup(context => context.ConnectionId)
                .Returns(Guid.NewGuid().ToString);
           
            _hub.Clients = _clientsMock.Object;
            _hub.Context = _clientContextMock.Object;

            // Act
            await _hub.RegisterNewPlayer("playerName");
           
            // Assert
            _clientsMock.Verify(clients => clients.Caller, Times.Once);
            _clientProxyMock
                .Verify(clientProxy => clientProxy.ReceivePlayerProfile(It.IsAny<string>()), Times.Once);
        }
    }
}