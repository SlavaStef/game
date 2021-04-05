﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.Common.Helpers.Media;
using Serilog;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task UpdateProfileImage(string imageJson, string playerIdJson)
        {
            Log.Information("UpdateProfileImage. Start");
            Log.Information($"UpdateProfileImage. imageJson: {imageJson}");
            var image = new Image
            {
                BinaryImage = JsonSerializer.Deserialize<byte[]>(imageJson),
                PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson)
            };
            var updateResult = await _mediaService.UpdateProfileImage(JsonSerializer.Serialize(image));
            Log.Information($"UpdateProfileImage. updateResult: {updateResult}");
            if (updateResult.IsSuccess is false)
            {
                Log.Error(
                    $"RemoveProfileImage. {updateResult.Message} PlayerId: {JsonSerializer.Deserialize<Image>(imageJson).PlayerId}");
                return;
            }

            await Clients.Caller.ReceiveProfileImage(JsonSerializer.Serialize(updateResult.Value));
            Log.Information("UpdateProfileImage. End");
        }

        public async Task GetProfileImage(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            var getResult = await _mediaService.GetProfileImage(playerId);

            if (getResult.IsSuccess is false)
            {
                Log.Error(
                    $"RemoveProfileImage. {getResult.Message} PlayerId: {playerId}");
                return;
            }
            
            await Clients.Caller.ReceiveProfileImage(JsonSerializer.Serialize(getResult.Value));
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
            
            await Clients.Caller.ReceiveProfileImage(JsonSerializer.Serialize(removeResult.Value));
        }
    }
}