using PokerHand.DataAccess.Context;
using SignalR_UnitTestingSupportXUnit.Hubs;

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