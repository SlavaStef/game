using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        
        //TODO: need to receive Json
        public async Task RegisterNewPlayer(string userName)
        {
            var playerProfileDto = await _playerService.CreatePlayer(userName);

            if (playerProfileDto is null)
                return;
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(playerProfileDto));

            _allPlayers.Add(playerProfileDto.Id, Context.ConnectionId);
            
            _logger.LogInformation($"Player {playerProfileDto.UserName} is registered");
        } 

        public async Task Authenticate(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            
            var playerProfileDto = await _playerService.Authenticate(playerId);

            if (playerProfileDto is null)
            {
                await Clients.Caller.ReceivePlayerNotFound();
                return;
            }

            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(playerProfileDto));

            _allPlayers.AddOrUpdate(playerId, Context.ConnectionId);
            
            _logger.LogInformation($"Player {playerProfileDto.UserName} is authenticated");
        }
    }
}