using System;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.Common.Helpers.Player;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task SendPlayerProfile(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);

            var connectionId = _allPlayers.GetValueByKey(playerId);

            if (connectionId != Context.ConnectionId)
                return;
            
            var profileDto = await _playerService.GetProfile(playerId);
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(profileDto));
        }

        public async Task UpdatePlayerProfile(string updateFormJson)
        {
            var profileDto =
                await _playerService.UpdateProfile(JsonSerializer.Deserialize<PlayerProfileUpdateForm>(updateFormJson));
            
            if (profileDto is null)
                return;
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(profileDto));
        }
    }
}