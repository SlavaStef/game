using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common;
using PokerHand.Common.Entities;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.BusinessLogic.Tests.Services
{
    public class PlayerServiceTests
    {
        private readonly IPlayerService _sut;
        private readonly Mock<IMediaService> _mediaServiceMock = new Mock<IMediaService>();
        private readonly Mock<ITablesOnline> _tablesOnlineMock = new Mock<ITablesOnline>();
        private readonly Mock<UserManager<Player>> _userManagerMock = new Mock<UserManager<Player>>();
        private readonly Mock<ILogger<PlayerService>> _loggerMock = new Mock<ILogger<PlayerService>>();
        private readonly Mock<IMapper> _mapperMock = new Mock<IMapper>();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>();

        public PlayerServiceTests()
        {
            _sut = new PlayerService(_userManagerMock.Object, _mapperMock.Object, _loggerMock.Object,
                _unitOfWorkMock.Object, _tablesOnlineMock.Object, _mediaServiceMock.Object);
        }

        public async Task TryGetPlayerProfileBySocialId_ReturnsFalse_IfThereIsNoUser()
        {
            var socialId = "3569124793216471932";
        }

        public void TryGetPlayerProfileBySocialId_ReturnsPlayerProfileDto_IfThereIsUser()
        {
            
        }
    }
}