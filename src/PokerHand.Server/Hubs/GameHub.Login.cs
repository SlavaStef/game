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
            
            var playerProfileDto = await _loginService.AuthenticateWithPlayerId(playerId);

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
            try
            {
                _logger.LogInformation("RegisterWithExternalProvider. Start");
                _logger.LogInformation($"RegisterWithExternalProvider. username: {userNameJson}");
                _logger.LogInformation($"RegisterWithExternalProvider. genderJson: {genderJson}");
                _logger.LogInformation($"RegisterWithExternalProvider. handsSpriteJson: {handsSpriteJson}");
                _logger.LogInformation($"RegisterWithExternalProvider. providerNameJson: {providerNameJson}");
                _logger.LogInformation($"RegisterWithExternalProvider. providerKeyJson: {providerKeyJson}");
                _logger.LogInformation($"RegisterWithExternalProvider. image: {image}");
                
                var newPlayerProfileDto =
                    await _playerService.CreatePlayer(JsonSerializer.Deserialize<string>(userNameJson),
                        JsonSerializer.Deserialize<Gender>(genderJson),
                        JsonSerializer.Deserialize<HandsSpriteType>(handsSpriteJson));

                if (newPlayerProfileDto is null)
                {
                    _logger.LogInformation($"RegisterWithExternalProvider. newPlayerProfileDto is null");
                    return;
                }
                
                _logger.LogInformation($"RegisterWithExternalProvider. newPlayerProfileDto: {newPlayerProfileDto}");
                
                await _loginService.CreateExternalLogin(newPlayerProfileDto.Id,
                    JsonSerializer.Deserialize<ExternalProviderName>(providerNameJson),
                    JsonSerializer.Deserialize<string>(providerKeyJson));
            
                _logger.LogInformation("RegisterWithExternalProvider. 3");
            
                await Clients.Caller
                    .ReceivePlayerProfile(JsonSerializer.Serialize(newPlayerProfileDto));

                await SendProfileImage(newPlayerProfileDto.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public async Task TryAuthenticateWithExternalProvider(string providerKeyJson)
        {
            try
            {
                _logger.LogInformation($"TryAuthenticateWithExternalProvider. Provider key: {providerKeyJson}");
                var authenticateResult = await _loginService
                    .TryAuthenticateWithExternalProvider(JsonSerializer.Deserialize<string>(providerKeyJson));

                _logger.LogInformation(
                    $"TryAuthenticateWithExternalProvider. Authentication result: {authenticateResult.IsSuccess}");
                if (authenticateResult.IsSuccess is false)
                {
                    _logger.LogInformation($"TryAuthenticateWithExternalProvider. Calling Continue registration");
                    await Clients.Caller
                        .ContinueRegistration();
                
                    return;
                }

                await Clients.Caller
                    .ReceivePlayerProfile(JsonSerializer.Serialize(authenticateResult.Value));

                await SendProfileImage(authenticateResult.Value.Id);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                _logger.LogInformation(e.StackTrace);
                throw;
            }
            
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