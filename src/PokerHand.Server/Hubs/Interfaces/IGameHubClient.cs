using System.Threading.Tasks;

namespace PokerHand.Server.Hubs.Interfaces
{
    public interface IGameHubClient
    {
        Task ReceiveConnectionId(string connectionId);
        Task ReceiveTotalMoney(string amount);
        Task ContinueRegistration();
        Task ReceivePlayerProfile(string playerProfileDto);
        Task ReceivePlayerNotFound();
        Task ReceiveTableInfo(string tableInfo);
        Task ReceiveAllTablesInfo(string allTablesInfo);
        Task ReceivePlayerDto(string playerDto);
        Task ReceiveTableState(string tableDto);
        Task OnGameEnd();
        Task ReceivePlayerAction(string actionFromPlayer);
        Task PlayerDisconnected(string tableDto);
        Task PrepareForGame(string tableDto);
        Task DealCommunityCards(string cardsList);
        Task ReceiveUpdatedPot(string newPotAmount);
        Task ReceiveCurrentPlayerIdInWagering(string currentPlayerIdJson);
        Task ReceiveWinners(string playerDtoListJson);
        Task EndSitAndGoGame(string place); //TODO: serialize
        Task OnLackOfStackMoney();

        // Media
        Task ReceiveProfileImage(string avatarJson);
        
        // Presents
        Task ReceivePresent(string present);
        Task ErrorOnSendPresent(string error);
        
        // QuickChat
        Task ReceiveQuickMessage(string messageTypeJson, string messageJson, string senderIndexJson);
    }
}