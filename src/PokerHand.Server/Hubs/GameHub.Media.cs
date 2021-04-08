using System;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.Common.Helpers.Media;
using Serilog;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task GetProfileImage(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            var getResult = await _mediaService.GetProfileImage(playerId);

            if (getResult.IsSuccess is false)
            {
                Log.Error(
                    $"GetProfileImage. {getResult.Message} PlayerId: {playerId}");
                return;
            }
            
            Log.Information($"GetProfileImage. Image: {getResult.Value}");
            var avatar = new Avatar
                {BinaryImage = getResult.Value, PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson)};
            
            await Clients.Caller
                .ReceiveProfileImage(JsonSerializer.Serialize(avatar));
        }
        
        public async Task UpdateProfileImage(string playerIdJson, string newProfileImage)
        {
            Log.Information("UpdateProfileImage. Start");
            Log.Information($"UpdateProfileImage. playerIdJson: {playerIdJson}, imageJson: {newProfileImage}");

            var updateResult =
                await _mediaService.UpdateProfileImage(JsonSerializer.Deserialize<Guid>(playerIdJson), newProfileImage);
            Log.Information($"UpdateProfileImage. updateResult: {updateResult}");
            
            if (updateResult.IsSuccess is false)
            {
                Log.Error($"RemoveProfileImage. {updateResult.Message} PlayerId: {JsonSerializer.Deserialize<Guid>(playerIdJson)}");
                return;
            }
            
            var avatar = new Avatar
                {BinaryImage = updateResult.Value, PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson)};
            
            await Clients.Caller
                .ReceiveProfileImage(JsonSerializer.Serialize(avatar));
            
            Log.Information("UpdateProfileImage. End");
        }
        
        public async Task RemoveProfileImage(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            var removeResult = await _mediaService.SetDefaultProfileImage(playerId);

            if (removeResult.IsSuccess is false)
            {
                Log.Error(
                    $"RemoveProfileImage. {removeResult.Message} PlayerId: {playerId}");
                return;
            }
            
            var avatar = new Avatar
                {BinaryImage = removeResult.Value, PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson)};
            
            await Clients.Caller
                .ReceiveProfileImage(JsonSerializer.Serialize(avatar));
        }
    }
}