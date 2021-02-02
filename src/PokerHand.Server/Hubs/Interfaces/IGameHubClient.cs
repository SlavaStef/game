using System.Threading.Tasks;
using PokerHand.Common.Dto.Chat;

namespace PokerHand.Server.Hubs.Interfaces
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
        Task ReceiveUpdatedPot(string newPotAmount);
        Task ReceiveCurrentPlayerIdInWagering(string id);
        Task ReceiveTableStateAtWageringEnd(string tableDto);
        Task ReceiveWinners(string playerDtoList);
        Task EndSitAndGoGame(string place);
        Task OnLackOfStackMoney();
        
        // Chat
        Task ReceivePrivateMessage(string privateMessageDto);
        Task ReceivePublicMessage(string publicMessageDto);
        Task ConfirmMessageWasSent();

    }
}