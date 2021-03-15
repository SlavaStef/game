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
        public async Task LeaveTable(string tableId, string playerId)
        {
            _logger.LogInformation("LeaveTable. Start");
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);

            var table = _allTables.GetById(tableIdGuid);
            var player = table?.Players.FirstOrDefault(p => p.Id == playerIdGuid);

            _logger.LogInformation( $"LeaveTable. tableId: {table.Id}, playerId: {player.Id}");
            
            // Remove player if it is his turn how
            if (table.CurrentPlayer?.Id == player.Id)
            {
                await RemoveCurrentPlayer(player, table);
                return;
            }

            var tableDto = await _tableService.RemovePlayerFromTable(tableIdGuid, playerIdGuid);

            if (tableDto != null)
                await Clients.GroupExcept(JsonSerializer.Deserialize<string>(tableId), Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(tableDto));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");
            WriteAllPlayersList();
            
            var playerId = _allPlayers.GetKeyByValue(Context.ConnectionId);
            var table = _allTables.GetByPlayerId(playerId);
            var player = table?.Players.FirstOrDefault(p => p.Id == playerId);

            if (table != null)
            {
                if (table.CurrentPlayer?.Id == player.Id)
                    await RemoveCurrentPlayer(player, table);
                else
                {
                    if (table.Players.Count is 1)
                        await _tableService.RemovePlayerFromTable(table.Id, playerId);
                    else
                    {
                        var tableDto = await _tableService.RemovePlayerFromTable(table.Id, playerId);

                        if (tableDto != null)
                            await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId)
                                .PlayerDisconnected(JsonSerializer.Serialize(tableDto));
                    }
                }
            }
            
            _allPlayers.Remove(playerId);
            
            WriteAllPlayersList();
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {playerId} : {Context.ConnectionId} removed from table");
        }
        
        private async Task RemoveCurrentPlayer(Player player, Table table)
        {
            _logger.LogInformation("RemoveCurrentPlayer. Inside IF");
            var foldAction = new PlayerAction
            {
                ActionType = PlayerActionType.Fold,
                PlayerIndexNumber = player.IndexNumber
            };

            _logger.LogInformation($"RemoveCurrentPlayer. action: {JsonSerializer.Serialize(foldAction)}");

            player.CurrentAction = foldAction;

            _logger.LogInformation($"RemoveCurrentPlayer. player.CurrentAction: {player.CurrentAction}");

            await Clients.GroupExcept(table.Id.ToString(), table.CurrentPlayer.ConnectionId)
                .ReceivePlayerAction(JsonSerializer.Serialize(table.CurrentPlayer.CurrentAction));

            _logger.LogInformation($"RemoveCurrentPlayer. Action is sent to all players");

            // Deal with player's state on table
            if (player.CurrentBet != 0)
                table.Pot.TotalAmount += player.CurrentBet;

            if (player.StackMoney > 0)
                await _playerService.AddTotalMoney(player.Id, player.StackMoney);

            if (table.Type is TableType.SitAndGo)
            {
                var playerPlace = table.Players.Count;
                _logger.LogInformation($"RemoveCurrentPlayer. playerPlace: {playerPlace}");
                
                await Clients.Client(player.ConnectionId)
                    .EndSitAndGoGame(playerPlace.ToString());
            }

            _logger.LogInformation($"RemoveCurrentPlayer. Before deleting from active players: {table.ActivePlayers.Count}");
            if (table.ActivePlayers.Contains(player))
                table.ActivePlayers.Remove(player);
            _logger.LogInformation($"RemoveCurrentPlayer. After deleting from active players: {table.ActivePlayers.Count}");

            _logger.LogInformation($"RemoveCurrentPlayer. Before deleting from players: {table.Players.Count}");

            table.Players.Remove(player);

            _logger.LogInformation($"RemoveCurrentPlayer. After deleting from players: {table.Players.Count}");

            await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId)
                .PlayerDisconnected(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            
            _logger.LogInformation($"RemoveCurrentPlayer. PlayerDisconnected was sent");

            await Groups.RemoveFromGroupAsync(player.ConnectionId, table.Id.ToString());
            _logger.LogInformation($"RemoveCurrentPlayer. Player was removed from group");

            table.WaitForPlayerBet.Set();

            _logger.LogInformation($"RemoveCurrentPlayer. WaitForPlayerBet was released");
        }
    }
}