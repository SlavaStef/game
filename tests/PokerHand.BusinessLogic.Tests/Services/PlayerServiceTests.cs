// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using AutoMapper;
// using FluentAssertions;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.Extensions.Logging;
// using Moq;
// using NSubstitute;
// using PokerHand.BusinessLogic.Interfaces;
// using PokerHand.BusinessLogic.Services;
// using PokerHand.Common;
// using PokerHand.Common.Entities;
// using PokerHand.Common.Helpers.Player;
// using PokerHand.DataAccess.Interfaces;
// using Xunit;
//
// namespace PokerHand.BusinessLogic.Tests.Services
// {
//     public class PlayerServiceTests
//     {
//         private readonly Mock<IMediaService> _mediaServiceMock = new Mock<IMediaService>();
//         private readonly Mock<ITablesOnline> _tablesOnline = new Mock<ITablesOnline>();
//         private readonly Mock<ILogger<PlayerService>> _loggerMock = new Mock<ILogger<PlayerService>>();
//         private readonly Mock<IMapper> _mapperMock = new Mock<IMapper>();
//         private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>();
//
//         [Fact]
//         public void CreatePlayer_ReturnsCountryBY_IfGetsBY()
//         {
//             
//         }
//         
//         [Fact]
//         public async Task CreatePlayer_ReturnsCountryNone_IfGetsCountryNotListedInEnum()
//         {
//             var store = new Mock<IUserStore<Player>>();
//             var userManager = new UserManager<Player>(store.Object, null, null, null, null, null, null, null, null);
//             
//             store
//                 .Setup(x => x.CreateAsync(It.IsAny<Player>(), CancellationToken.None))
//                 .Returns(Task.FromResult(IdentityResult.Success));
//             
//             _mediaServiceMock
//                 .Setup(x => x.SetDefaultProfileImage(It.IsAny<Guid>()))
//                 .Returns(new ResultModel<byte[]> { IsSuccess = true, Value = new byte[2] });
//         
//             var sut = new PlayerService(userManager, _mapperMock.Object, _loggerMock.Object, _unitOfWorkMock.Object,
//                 _tablesOnline.Object, _mediaServiceMock.Object);
//
//             var result = await sut.CreatePlayer("testName", Gender.Male, HandsSpriteType.WhiteMan, "5.182.184.0");
//
//             result.Country.Should().Be(CountryCode.None);
//         }
//     }
// }