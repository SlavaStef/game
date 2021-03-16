using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task LeaveTable(string tableIdJson, string playerIdJson)
        {
            _logger.LogInformation("LeaveTable. Start");
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);

            var table = _allTables.GetById(tableId);
            var player = table?.Players.FirstOrDefault(p => p.Id == playerId);

            _logger.LogInformation( $"LeaveTable. tableId: {table.Id}, playerId: {player.Id}");

            var removeResult = await _tableService.RemovePlayerFromTable(tableId, playerId);
            if (removeResult.IsSuccess is false)
            {
                _logger.LogError($"{removeResult.Message}");
                return;
            }
            
            if (removeResult.Value.WasTableRemoved is false)
                await Clients.GroupExcept(JsonSerializer.Deserialize<string>(tableIdJson), Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(removeResult.Value.TableDto));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");
            WriteAllPlayersList();
            
            var playerId = _allPlayers.GetKeyByValue(Context.ConnectionId);
            var table = _allTables.GetByPlayerId(playerId);
            if (table is null)
            {
                _logger.LogInformation($"No table found for player {playerId}");
                _allPlayers.Remove(playerId);
                return;
            }
            
            var removeResult = await _tableService.RemovePlayerFromTable(table.Id, playerId);
            if (removeResult.IsSuccess is false)
            {
                _logger.LogError($"{removeResult.Message}");
                return;
            }
            
            if (removeResult.Value.WasTableRemoved is false)
                await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(removeResult.Value.TableDto));
            
            _allPlayers.Remove(playerId);
            
            WriteAllPlayersList();
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {playerId} : {Context.ConnectionId} removed from table");
        }
    }
}