using System;
using System.Threading.Tasks;

namespace PokerHand.Server.Hubs.Interfaces
{
    public interface IGameHub
    {
        Task OnConnectedAsync();
        Task ConnectToTable(string tableTitleJson, string playerIdJson, string buyInJson, string isAutoTopJson);
        void ReceivePlayerActionFromClient(string actionJson, string tableIdJson);
        void ReceiveActivePlayerStatus(string tableId, string playerId);
        Task ReceiveNewBuyIn(string tableIdJson, string playerIdJson, string amountJson, string isAutoTopJson);

        // Disconnect
        Task LeaveTable(string tableIdJson, string playerIdJson);
        Task SwitchTable(string tableTitleJson, string playerIdJson, string currentTableIdJson, string buyInJson,
            string isAutoTopJson);
        Task OnDisconnectedAsync(Exception exception);
        
        // QuickChat
        Task SendQuickMessage(string quickMessageJson, string tableIdString);
    }
}