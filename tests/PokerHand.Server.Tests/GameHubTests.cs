using System.Threading;
using Moq;
using PokerHand.DataAccess.Context;
using PokerHand.Server.Hubs;
using SignalR_UnitTestingSupportXUnit.Hubs;
using Xunit;

namespace PokerHand.Server.Tests
{
    public class GameHubTests  : HubUnitTestsWithEF<ApplicationContext>
    {
        // private readonly GameHub _hub;
        //
        // public GameHubTests()
        // {
        //     _hub = new GameHub();
        //     AssignToHubRequiredProperties(_hub);
        // }
        //
        // [Fact]
        // public void GetTableInfo_ReturnsTableInfo()
        // {
        //     _hub.RegisterNewPlayer("TestName");
        //     ClientsCallerMock
        //         .Verify(x => x.SendCoreAsync("ReceivePlayerProfile", new object[] {"TestArgument"}, It.IsAny<CancellationToken>()));
        // }
    }
}