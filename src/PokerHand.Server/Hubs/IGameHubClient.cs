using System.Threading.Tasks;

namespace PokerHand.Server.Hubs
{
    public interface IGameHubClient
    {
        // Methods, used in hub
        Task ReceivePlayerProfile(string playerProfileDto);
        Task ReceivePlayerNotFound();
        Task ReceiveTableInfo(string tableInfo);
        Task ReceiveAllTablesInfo(string allTablesInfo);
        Task ReceivePlayerDto(string playerDto);
        Task ReceiveTableState(string tableDto);
        Task ReceivePlayerAction(string actionFromPlayer);
        Task PlayerDisconnected(string tableDto);

        // Methods, used in game process manager
        Task WaitForPlayers();
        Task SetDealerAndBlinds(string tableDto);
        Task DealPocketCards(string playerDtoList);
        Task DealCommunityCards(string cardsList);
        Task ReceiveCurrentPlayerIdInWagering(string id);
        Task ReceiveTableStateAtWageringEnd(string tableDto);
        Task ReceiveWinners(string playerDtoList);
        
    }
}