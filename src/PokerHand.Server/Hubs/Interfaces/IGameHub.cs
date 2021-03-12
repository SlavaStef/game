using System;
using System.Threading.Tasks;

namespace PokerHand.Server.Hubs.Interfaces
{
    public interface IGameHub
    {
        Task OnConnectedAsync();
        Task RegisterNewPlayer(string userName);
        Task Authenticate(string playerId);
        Task GetTableInfo(string tableTitle);
        Task GetAllTablesInfo();
        Task ConnectToTable(string tableTitle, string playerId, string buyInAmount, string isAutoTop);
        Task ReceivePlayerActionFromClient(string actionFromPlayer, string tableIdFromPlayer);
        void ReceiveActivePlayerStatus(string tableId, string playerId);
        Task LeaveTable(string tableId, string playerId);
        Task OnDisconnectedAsync(Exception exception);
        
        // Media
        Task UpdateProfileImage(string imageJson);
        Task GetProfileImage(string playerIdJson);
        Task RemoveProfileImage(string playerIdJson);
    }
}