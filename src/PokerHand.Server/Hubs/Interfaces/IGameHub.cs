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
        Task OnDisconnectedAsync(Exception exception);
        
        Task GetTableInfo(string tableTitle);
        Task GetAllTablesInfo();
        Task ConnectToTable(string tableTitleJson, string playerIdJson, string buyInAmountJson, string isAutoTopJson);
        void ReceivePlayerActionFromClient(string actionJson, string tableIdJson);
        Task SendPlayerProfile(string playerIdJson);
        void ReceiveActivePlayerStatus(string tableId, string playerId);
        Task AddStackMoney(string tableIdJson, string playerIdJson, string amountJson);
        
        // Media
        Task UpdateProfileImage(string imageJson);
        Task GetProfileImage(string playerIdJson);
        Task RemoveProfileImage(string playerIdJson);
    }
}