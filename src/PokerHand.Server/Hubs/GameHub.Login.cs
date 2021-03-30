using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.Common.Helpers.Player;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
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
        
        public async Task RegisterAsGuest(string userNameJson, string genderJson, string handsSpriteJson)
        {
            var newPlayerProfileDto =
                await _playerService.CreatePlayer(JsonSerializer.Deserialize<string>(userNameJson),
                    JsonSerializer.Deserialize<Gender>(genderJson), 
                    JsonSerializer.Deserialize<HandsSpriteType>(handsSpriteJson));

            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(newPlayerProfileDto));

            await SendProfileImage(newPlayerProfileDto.Id);
        }
        
        public async Task RegisterWithExternalProvider(string userNameJson, string genderJson, string handsSpriteJson, 
            string providerNameJson, string providerKeyJson, string image)
        {
            _logger.LogInformation("RegisterWithExternalProvider. 1");
            _logger.LogInformation($"image: {image}");
            var newPlayerProfileDto =
                await _playerService.CreatePlayer(JsonSerializer.Deserialize<string>(userNameJson),
                    JsonSerializer.Deserialize<Gender>(genderJson),
                    JsonSerializer.Deserialize<HandsSpriteType>(handsSpriteJson));

            _logger.LogInformation("RegisterWithExternalProvider. 2");
            
            await _loginService.CreateExternalLogin(newPlayerProfileDto.Id,
                JsonSerializer.Deserialize<ExternalProviderName>(providerNameJson),
                JsonSerializer.Deserialize<string>(providerKeyJson));
            
            _logger.LogInformation("RegisterWithExternalProvider. 3");
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(newPlayerProfileDto));

            await SendProfileImage(newPlayerProfileDto.Id);
        }

        public async Task TryAuthenticateWithExternalProvider(string providerKeyJson)
        {
            _logger.LogInformation($"provider key: {providerKeyJson}");
            var authenticateResult = await _loginService
                .TryAuthenticate(JsonSerializer.Deserialize<string>(providerKeyJson));

            if (authenticateResult.IsSuccess is false)
            {
                await Clients.Caller
                    .ContinueRegistration();
                
                return;
            }

            await Clients.Caller
                    .ReceivePlayerProfile(JsonSerializer.Serialize(authenticateResult.Value));

            await SendProfileImage(authenticateResult.Value.Id);
        }

        #region Helpers
        
        private async Task SendProfileImage(Guid playerId)
        {
            var getImageResult = await _mediaService.GetProfileImage(playerId);

            if (getImageResult.IsSuccess is false)
            {
                _logger.LogInformation($"{getImageResult.Message}");
                return;
            }

            await Clients.Caller
                .ReceiveProfileImage(JsonSerializer.Serialize(getImageResult.Value));
        }

        #endregion
    }
}