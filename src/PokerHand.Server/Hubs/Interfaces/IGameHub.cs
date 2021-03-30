using System;
using System.Threading.Tasks;

namespace PokerHand.Server.Hubs.Interfaces
{
    public interface IGameHub
    {
        // Login
        Task OnConnectedAsync();
        Task Authenticate(string playerIdJson);
        Task RegisterAsGuest(string userNameJson, string genderJson, string handsSpriteJson);
        Task RegisterWithExternalProvider(string userNameJson, string genderJson, string handsSpriteJson, 
            string providerNameJson, string providerKeyJson, string image);
        Task TryAuthenticateWithExternalProvider(string providerKeyJson);
        
        // Disconnect
        Task LeaveTable(string tableIdJson, string playerIdJson);

        Task SwitchTable(string tableTitleJson, string playerIdJson, string currentTableIdJson, string buyInJson,
            string isAutoTopJson);
        Task OnDisconnectedAsync(Exception exception);
        
        Task GetTableInfo(string tableTitle);
        Task GetAllTablesInfo();
        Task ConnectToTable(string tableTitleJson, string playerIdJson, string buyInJson, string isAutoTopJson);
        void ReceivePlayerActionFromClient(string actionJson, string tableIdJson);
        void ReceiveActivePlayerStatus(string tableId, string playerId);
        Task ReceiveNewBuyIn(string tableIdJson, string playerIdJson, string amountJson, string isAutoTopJson);
        
        // Profile
        Task SendPlayerProfile(string playerIdJson);
        Task UpdatePlayerProfile(string updateFormJson);
        
        // Media
        Task UpdateProfileImage(string imageJson);
        Task GetProfileImage(string playerIdJson);
        Task RemoveProfileImage(string playerIdJson);
    }
}