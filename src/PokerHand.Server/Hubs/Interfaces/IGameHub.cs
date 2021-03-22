using System;
using System.Threading.Tasks;

namespace PokerHand.Server.Hubs.Interfaces
{
    public interface IGameHub
    {
        // Login
        Task OnConnectedAsync();
        Task RegisterNewPlayer(string userName);
        Task Authenticate(string playerIdJson);
        
        // Disconnect
        Task LeaveTable(string tableIdJson, string playerIdJson);
        Task SwitchTable(string currentTableIdJson, string connectionOptionsJson);
        Task OnDisconnectedAsync(Exception exception);
        
        Task GetTableInfo(string tableTitle);
        Task GetAllTablesInfo();
        Task ConnectToTable(string connectionOptions);
        void ReceivePlayerActionFromClient(string actionJson, string tableIdJson);
        Task SendPlayerProfile(string playerIdJson);
        void ReceiveActivePlayerStatus(string tableId, string playerId);
        Task ReceiveNewBuyIn(string tableIdJson, string playerIdJson, string amountJson, string isAutoTopJson);
        
        // Media
        Task UpdateProfileImage(string imageJson);
        Task GetProfileImage(string playerIdJson);
        Task RemoveProfileImage(string playerIdJson);
    }
}