using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerHand.Common.Helpers.Table;
using Serilog;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task LeaveTable(string tableIdJson, string playerIdJson)
        {
            Log.Information("LeaveTable. Start");
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);

            var table = _allTables.GetById(tableId);
            var player = table?.Players.FirstOrDefault(p => p.Id == playerId);
            if (player is null)
                return;

            Log.Information( $"LeaveTable. tableId: {table.Id}, playerId: {player.Id}");

            var removeResult = await _tableService.RemovePlayerFromTable(tableId, playerId);
            if (removeResult.IsSuccess is false)
            {
                Log.Error($"{removeResult.Message}");
                return;
            }
            
            if (removeResult.Value.WasTableRemoved is false)
                await Clients.GroupExcept(JsonSerializer.Deserialize<string>(tableIdJson), Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(removeResult.Value.TableDto));
        }

        public async Task SwitchTable(string tableTitleJson, string playerIdJson,  string currentTableIdJson, string buyInJson, string isAutoTopJson)
        {
            var connectionOptions = new TableConnectionOptions
            {
                TableTitle = JsonSerializer.Deserialize<TableTitle>(tableTitleJson),
                PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson),
                PlayerConnectionId = Context.ConnectionId,
                CurrentTableId = JsonSerializer.Deserialize<Guid>(currentTableIdJson),
                BuyInAmount = JsonSerializer.Deserialize<int>(buyInJson),
                IsAutoTop = JsonSerializer.Deserialize<bool>(isAutoTopJson)
            };

            var validationResult = await new TableConnectionOptionsValidator().ValidateAsync(connectionOptions);
            if (validationResult.IsValid is not true)
            {
                foreach (var error in validationResult.Errors)
                    Log.Error($"{error.ErrorMessage}");

                return;
            }

            await _tableService.RemovePlayerFromTable((Guid)connectionOptions.CurrentTableId, connectionOptions.PlayerId);

            var connectionResult = await _tableService.AddPlayerToTable(connectionOptions);
            if (connectionResult.IsSuccess is false)
            {
                Log.Error($"SwitchTable Error: {connectionResult.Message}");
                return;
            }
            
            await AddPlayerToTableGroup(connectionResult);

            //if (connectionResult.Value.IsNewTable is true) 
                StartGameOnNewTable(connectionResult.Value.TableDto.Id);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Log.Information($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");
            WriteAllPlayersList();
            
            var playerId = _allPlayers.GetKeyByValue(Context.ConnectionId);
            var table = _allTables.GetByPlayerId(playerId);
            if (table is null)
            {
                Log.Information($"No table found for player {playerId}");
                _allPlayers.Remove(playerId);
                return;
            }
            
            var removeResult = await _tableService.RemovePlayerFromTable(table.Id, playerId);
            if (removeResult.IsSuccess is false)
            {
                Log.Error($"{removeResult.Message}");
                return;
            }
            
            if (removeResult.Value.WasTableRemoved is false)
                await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(removeResult.Value.TableDto));
            
            _allPlayers.Remove(playerId);
            
            WriteAllPlayersList();
            Log.Information($"GameHub.OnDisconnectedAsync. Player {playerId} : {Context.ConnectionId} removed from table");
        }
    }
}