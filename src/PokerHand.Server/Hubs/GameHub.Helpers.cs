using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerHand.Common;
using PokerHand.Common.Dto;
using Serilog;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        private async Task AddPlayerToTableGroup(ResultModel<ConnectToTableResult> connectionResult)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connectionResult.Value.TableDto.Id.ToString());
            await Clients.Caller
                .ReceivePlayerDto(JsonSerializer.Serialize(connectionResult.Value.PlayerDto));
            await Clients.Group(connectionResult.Value.TableDto.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(connectionResult.Value.TableDto));
        }
        
        private void StartGameOnNewTable(Guid id) =>
            new Thread(() => _gameProcessService.StartRound(id)).Start();
        
        private void WriteAllPlayersList() =>
            Log.Information($"AllPlayers: {JsonSerializer.Serialize(_allPlayers.GetAll())}");
        
        private void RegisterEventHandlers()
        {
            _gameProcessService.OnPrepareForGame += async table =>
            {
                await Clients.Group(table.Id.ToString())
                    .PrepareForGame(JsonSerializer.Serialize(_mapper.Map<TableDto>(table)));
            };

            _gameProcessService.OnDealCommunityCards += async (table, cardsToAdd) =>
            {
                await Clients.Group(table.Id.ToString())
                    .DealCommunityCards(JsonSerializer.Serialize(cardsToAdd));
            };

            _gameProcessService.ReceiveWinners += async (table, sidePotsJson) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceiveWinners(sidePotsJson);
            };

            _gameProcessService.ReceiveUpdatedPot += async (table, potJson) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceiveUpdatedPot(potJson);
            };

            _gameProcessService.ReceivePlayerAction += async (table, action) =>
            {
                Log.Information($"action: {action} : {JsonSerializer.Serialize(action)}");
                
                await Clients.GroupExcept(table.Id.ToString(), table.CurrentPlayer.ConnectionId)
                    .ReceivePlayerAction(JsonSerializer.Serialize(action));
            };
            
            _tableService.ReceivePlayerAction += async (table, action) =>
            {
                Log.Information($"action: {action} : {JsonSerializer.Serialize(action)}");
                
                await Clients.GroupExcept(table.Id.ToString(), table.CurrentPlayer.ConnectionId)
                    .ReceivePlayerAction(JsonSerializer.Serialize(action));
            };

            _gameProcessService.ReceiveCurrentPlayerIdInWagering += async (table, currentPlayerIdJson) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceiveCurrentPlayerIdInWagering(currentPlayerIdJson);
            };
            
                _gameProcessService.BigBlindBetEvent += async (table, bigBlindAction) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceivePlayerAction(bigBlindAction);
            };
            
            _gameProcessService.SmallBlindBetEvent += async (table, smallBlindAction) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceivePlayerAction(smallBlindAction);
            };
            
            _gameProcessService.NewPlayerBetEvent += async (table, newPlayerBlindAction) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceivePlayerAction(newPlayerBlindAction);
            };
            
            _gameProcessService.ReceiveTableState += async tableToSend =>
            {
                await Clients.Group(tableToSend.Id.ToString())
                    .ReceiveTableState(JsonSerializer.Serialize(_mapper.Map<TableDto>(tableToSend)));
            };

            _gameProcessService.OnLackOfStackMoney += async player =>
            {
                await Clients.Client(player.ConnectionId)
                    .OnLackOfStackMoney();
            };

            _gameProcessService.ReceivePlayerDto += async player =>
            {
                await Clients.Client(player.ConnectionId)
                    .ReceivePlayerDto(JsonSerializer.Serialize(_mapper.Map<PlayerDto>(player)));
            };

            _gameProcessService.EndSitAndGoGameFirstPlace += async table =>
            {
                await Clients.Client(table.ActivePlayers[0].ConnectionId)
                    .EndSitAndGoGame("1");
            };
            
            _gameProcessService.EndSitAndGoGame += async (player, playersPlace) =>
            {
                await Clients.Client(player.ConnectionId)
                    .EndSitAndGoGame(playersPlace.ToString());
            };
            
            _tableService.EndSitAndGoGame += async (player, playersPlace) =>
            {
                await Clients.Client(player.ConnectionId)
                    .EndSitAndGoGame(playersPlace.ToString());
            };

            _gameProcessService.RemoveFromGroupAsync += async (connectionId, groupName) =>
            {
                Log.Information($"RemoveFromGroupAsync. connectionId: {connectionId}, groupName: {groupName}");
                await Groups
                    .RemoveFromGroupAsync(connectionId, groupName);
            };
            
            _tableService.RemoveFromGroupAsync += async (connectionId, groupName) =>
            {
                Log.Information($"RemoveFromGroupAsync. connectionId: {connectionId}, groupName: {groupName}");
                await Groups
                    .RemoveFromGroupAsync(connectionId, groupName);
            };

            _gameProcessService.OnGameEnd += async table =>
            {
                await Clients.Group(table.Id.ToString())
                    .OnGameEnd();
            };

            _gameProcessService.PlayerDisconnected += async (tableId, tableDtoJson) =>
            {
                await Clients.Group(tableId)
                    .PlayerDisconnected(tableDtoJson);
            };
            
            _tableService.PlayerDisconnected += async (tableId, tableDtoJson) =>
            {
                await Clients.Group(tableId)
                    .PlayerDisconnected(tableDtoJson);
            };

            _presentService.OnSendPresent += async (present) =>
            {
                await Clients.All.ReceivePresent(present);
            };
        }
    }
}