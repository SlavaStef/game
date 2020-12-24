using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Server.Helpers;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly List<Table> _allTables;
        private readonly ITableService _tableService;
        private readonly IPlayerService _playerService;
        private readonly IGameProcessManager _gameProcessManager;
        private readonly ILogger<GameHub> _logger;
        
        public GameHub(
            TablesCollection tablesCollection,
            ITableService tableService,
            IPlayerService playerService,
            IGameProcessManager gameProcessManager,
            ILogger<GameHub> logger)
        {
            _allTables = tablesCollection.Tables;
            _tableService = tableService;
            _playerService = playerService;
            _gameProcessManager = gameProcessManager;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"GameHub. Player {Context.ConnectionId} connected");
        }

        public async Task RegisterNewPlayer(string userName)
        {
            _logger.LogInformation("RegisterNewPlayer. Start");
            var playerProfileDto = await _playerService.AddNewPlayer(userName, _logger);
            
            await Clients.Caller.SendAsync("ReceivePlayerProfile", JsonSerializer.Serialize(playerProfileDto));
            _logger.LogInformation("RegisterNewPlayer. Start");
        }

        public async Task Authenticate(string playerId)
        {
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            
            var playerProfileDto = await _playerService.Authenticate(playerIdGuid);

            await Clients.Caller.SendAsync("ReceivePlayerProfile", JsonSerializer.Serialize(playerProfileDto));
        }

        public async Task GetTableInfo(string tableTitle)
        {
            var tableInfo = _tableService.GetTableInfo(tableTitle);
            await Clients.Caller.SendAsync("ReceiveTableInfo", JsonSerializer.Serialize(tableInfo));
        }
        
        public async Task ConnectToTable(string tableTitle, string playerId, string buyInAmount)
        {
            var title = JsonSerializer.Deserialize<TableTitle>(tableTitle);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            var buyIn = JsonSerializer.Deserialize<int>(buyInAmount);
            
            var (tableDto, isNewTable, playerDto) = await _tableService.AddPlayerToTable(title, playerIdGuid, buyIn);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, tableDto.Id.ToString());
            await Clients.Caller.SendAsync("ReceivePlayerDto", playerDto);
            await Clients.Group(tableDto.Id.ToString()).SendAsync("ReceiveTableState", tableDto);

            if (isNewTable)
            {
                _logger.LogInformation($"GameHub. New game started on a new table {tableDto.Id}");
                new Thread (() => _gameProcessManager.StartRound(tableDto.Id)).Start();
            }
        }
        
        public async void ReceivePlayerActionFromClient(string actionFromPlayer, string tableIdFromPlayer)
        {
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Start");
            
            var action = JsonSerializer.Deserialize<PlayerAction>(actionFromPlayer);
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdFromPlayer);
            
            await Clients.OthersInGroup(tableId.ToString()).SendAsync("ReceivePlayerAction", actionFromPlayer);
            
            var table = _allTables
                .First(t => t.Id == tableId);
            
            table.ActivePlayers
                .First(p => p.IndexNumber == action.PlayerIndexNumber)
                .CurrentAction = action;
            
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Action is added to Current player");
            _logger.LogInformation(JsonSerializer.Serialize(_allTables.First(t => t.Id == tableId)));
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Player's action received, sent to all players and added to entity.");
            //table.Mutex.ReleaseMutex();
            table.WaitForPlayerBet.Set();
            //WaitForPlayerBet.Set();
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. End");
            _logger.LogInformation($"THIS {JsonSerializer.Serialize(_allTables.First(t => t.Id == tableId))}");
        }

        public void ReceiveActivePlayerStatus(string tableId, string playerId)
        {
            var tableGuidId = Guid.Parse(tableId);
            var playerGuidId = Guid.Parse(playerId);

            _allTables
                .First(t => t.Id == tableGuidId)
                .Players
                .First(p => p.Id == playerGuidId).IsReady = true;
        }

        public async void LeaveTable(string tableId, string playerId)
        {
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);

            var tableDto = await _tableService.RemovePlayerFromTable(tableIdGuid, playerIdGuid);

            if (tableDto != null)
                await Clients.Group(tableId).SendAsync("PlayerDisconnected", JsonSerializer.Serialize(tableDto));
        }

        // public override async Task OnDisconnectedAsync(Exception exception)
        // {
        //     _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");
        //     
        //     var (table, isPlayerRemoved) = _tableService.RemovePlayerFromTable(Context.ConnectionId);
        //     
        //     if(isPlayerRemoved) 
        //         _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {Context.ConnectionId} removed from table");
        //     
        //
        //     if (table != null && isPlayerRemoved)
        //     {
        //         await Clients.Group(table.Id.ToString())
        //             .SendAsync("PlayerDisconnected", JsonSerializer.Serialize<TableDto>(table));
        //         _logger.LogInformation($"GameHub.OnDisconnectedAsync. Table: {table.Id}. Players: {table.Players.Count}");
        //
        //         foreach (var player in table.Players)
        //         {
        //             _logger.LogInformation($"Player: {player.Id}, name {player.UserName}");
        //         }
        //     }
        //     
        //     if(table == null)
        //         _logger.LogInformation($"GameHub.OnDisconnectedAsync. Table removed");
        // }
    }
}