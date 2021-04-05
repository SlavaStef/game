using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.Common.Helpers.Player;
using Serilog;

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
            
            Log.Information($"Player {playerProfileDto.UserName} is authenticated");
        }
        
        public async Task RegisterAsGuest(string userNameJson, string genderJson, string handsSpriteJson)
        {
            var newPlayerProfileDto =
                await _playerService.CreatePlayer(JsonSerializer.Deserialize<string>(userNameJson),
                    JsonSerializer.Deserialize<Gender>(genderJson), 
                    JsonSerializer.Deserialize<HandsSpriteType>(handsSpriteJson),
                    Context.GetHttpContext().Connection.RemoteIpAddress.ToString());

            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(newPlayerProfileDto));

            await SendProfileImage(newPlayerProfileDto.Id);
        }
        
        public async Task RegisterWithExternalProvider(string userNameJson, string genderJson, string handsSpriteJson, 
            string providerNameJson, string providerKeyJson, string image)
        {
            try
            {
                Log.Information("RegisterWithExternalProvider. Start");
                Log.Information($"RegisterWithExternalProvider. username: {userNameJson}");
                Log.Information($"RegisterWithExternalProvider. genderJson: {genderJson}");
                Log.Information($"RegisterWithExternalProvider. handsSpriteJson: {handsSpriteJson}");
                Log.Information($"RegisterWithExternalProvider. providerNameJson: {providerNameJson}");
                Log.Information($"RegisterWithExternalProvider. providerKeyJson: {providerKeyJson}");
                Log.Information($"RegisterWithExternalProvider. image: {image}");
                
                var newPlayerProfileDto =
                    await _playerService.CreatePlayer(JsonSerializer.Deserialize<string>(userNameJson),
                        JsonSerializer.Deserialize<Gender>(genderJson),
                        JsonSerializer.Deserialize<HandsSpriteType>(handsSpriteJson),
                        Context.GetHttpContext().Connection.RemoteIpAddress.ToString());

                if (newPlayerProfileDto is null)
                {
                    Log.Information($"RegisterWithExternalProvider. newPlayerProfileDto is null");
                    return;
                }
                
                Log.Information($"RegisterWithExternalProvider. newPlayerProfileDto: {newPlayerProfileDto}");
                
                await _loginService.CreateExternalLogin(newPlayerProfileDto.Id,
                    JsonSerializer.Deserialize<ExternalProviderName>(providerNameJson),
                    JsonSerializer.Deserialize<string>(providerKeyJson));
            
                Log.Information("RegisterWithExternalProvider. 3");
            
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
                Log.Information($"TryAuthenticateWithExternalProvider. Provider key: {providerKeyJson}");
                var authenticateResult = await _loginService
                    .TryAuthenticateWithExternalProvider(JsonSerializer.Deserialize<string>(providerKeyJson));

                Log.Information(
                    $"TryAuthenticateWithExternalProvider. Authentication result: {authenticateResult.IsSuccess}");
                if (authenticateResult.IsSuccess is false)
                {
                    Log.Information($"TryAuthenticateWithExternalProvider. Calling Continue registration");
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
                Log.Information(e.Message);
                Log.Information(e.StackTrace);
                throw;
            }
            
        }

        public async Task DeleteExternalProvider(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            await _loginService.DeleteExternalLogin(playerId);

            var newPlayerProfile = await _playerService.GetProfile(playerId);
            await Clients.Caller.ReceivePlayerProfile(JsonSerializer.Serialize(newPlayerProfile));
        }
        
        #region Helpers
        
        private async Task SendProfileImage(Guid playerId)
        {
            var getImageResult = await _mediaService.GetProfileImage(playerId);

            if (getImageResult.IsSuccess is false)
            {
                Log.Information($"{getImageResult.Message}");
                return;
            }

            await Clients.Caller
                .ReceiveProfileImage(JsonSerializer.Serialize(getImageResult.Value));
        }

        #endregion
    }
}