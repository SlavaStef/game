using AutoMapper;
using Moq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using Xunit;

namespace PokerHand.BusinessLogic.Tests.Services
{
    public class TableServiceTests
    {
        private readonly Mock<ITableService> _tableServiceMock = new();
        private readonly Mock<IPlayerService> _playerServiceMock = new();
        private readonly Mock<IPlayersOnline> _playersOnlineMock = new();
        private readonly Mock<ITablesOnline> _tablesOnlineMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IMediaService> _mediaServiceMock = new();
        private readonly Mock<IGameProcessService> _gameProcessServiceMock = new();
        
        [Fact]
        public void AddPlayerToTable()
        {
            
        }
    }
}