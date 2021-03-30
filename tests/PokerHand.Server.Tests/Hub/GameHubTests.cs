using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.Player;
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
        private readonly Mock<ILoginService> _loginServiceMock = new();

        public GameHubTests()
        {
            _hub = new GameHub(_tableServiceMock.Object, _playerServiceMock.Object, _loggerMock.Object,
                _playersOnlineMock.Object, _tablesOnlineMock.Object, _mapperMock.Object, _mediaServiceMock.Object,
                _gameProcessServiceMock.Object, _loginServiceMock.Object);
        }

        [Fact]
        public async Task RegisterNewPlayer_ReturnsSerializedPlayerDto_ToCaller()
        {
            // Arrange
            _playerServiceMock
                .Setup(x => x.CreatePlayer("playerName", Gender.Male, HandsSpriteType.BlackMan))
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
            await _hub.RegisterAsGuest("playerName", JsonSerializer.Serialize(Gender.Male),
                JsonSerializer.Serialize(HandsSpriteType.BlackMan));

            // Assert
            _clientsMock.Verify(clients => clients.Caller, Times.Once);
            _clientProxyMock
                .Verify(clientProxy => clientProxy.ReceivePlayerProfile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Test()
        {
            TestServer server = null;
            const string message = "This is a test message";
            var echo = string.Empty;

            var webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddSignalR();
                })
                .Configure(app =>
                {
                    app.UseRouting(); 
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHub<GameHub>("/game");
                    });
                }); 

            server = new TestServer(webHostBuilder);
            
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:54321/game", o => o.HttpMessageHandlerFactory = _ => server.CreateHandler())
                .Build();

            connection.On<string>("ReceiveTestMessage", receivedMessage => { echo = receivedMessage; });

            await connection.StartAsync();
            await connection.InvokeAsync("Test", message);

            echo.Should().Be(message);
        }
        
        
    }
}