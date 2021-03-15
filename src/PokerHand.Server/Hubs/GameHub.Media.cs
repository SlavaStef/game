using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PokerHand.Common.Helpers.Media;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task UpdateProfileImage(string imageJson)
        {
            var updateResult = await _mediaService.UpdateProfileImage(imageJson);

            if (updateResult.IsSuccess is false)
            {
                _logger.LogError(
                    $"RemoveProfileImage. {updateResult.Message} PlayerId: {JsonSerializer.Deserialize<Image>(imageJson).PlayerId}");
                return;
            }

            await Clients.Caller.ReceiveProfileImage(JsonSerializer.Serialize(updateResult.Value));
        }

        public async Task GetProfileImage(string playerIdJson)
        {
            var getResult = await _mediaService.GetProfileImage(playerIdJson);

            if (getResult.IsSuccess is false)
            {
                _logger.LogError(
                    $"RemoveProfileImage. {getResult.Message} PlayerId: {JsonSerializer.Deserialize<Guid>(playerIdJson)}");
                return;
            }
            
            await Clients.Caller.ReceiveProfileImage(JsonSerializer.Serialize(getResult.Value));
        }

        public async Task RemoveProfileImage(string playerIdJson)
        {
            var removeResult = await _mediaService.SetDefaultProfileImage(playerIdJson);

            if (removeResult.IsSuccess is false)
            {
                _logger.LogError(
                    $"RemoveProfileImage. {removeResult.Message} PlayerId: {JsonSerializer.Deserialize<Guid>(playerIdJson)}");
                return;
            }
            
            await Clients.Caller.ReceiveProfileImage(JsonSerializer.Serialize(removeResult.Value));
        }
    }
}