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
        private readonly IGameService _gameService;
        private readonly IGameProcessManager _gameProcessManager;
        private readonly ILogger<GameHub> _logger;
        
        public GameHub(
            TablesCollection tablesCollection,
            IGameService gameService,
            IGameProcessManager gameProcessManager,
            ILogger<GameHub> logger)
        {
            _allTables = tablesCollection.Tables;
            _gameProcessManager = gameProcessManager;
            _gameService = gameService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {    
            _logger.LogInformation($"GameHub. Player {Context.ConnectionId} connected");
        }

        public async Task GetTableInfo(string tableTitle)
        {
            var tableInfo = _gameService.GetTableInfo(tableTitle);
            await Clients.Caller.SendAsync("ReceiveTableInfo", JsonSerializer.Serialize(tableTitle));
        }
        
        public async Task ConnectToTable(string tableTitle, string buyInAmount)
        {
            var title = JsonSerializer.Deserialize<TableTitle>(tableTitle);
            var buyIn = JsonSerializer.Deserialize<int>(buyInAmount);
            
            var (table, isNewTable, player) = _gameService.AddPlayerToTable(Context.ConnectionId, title, buyIn);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, table.Id.ToString());
            await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId)
                .SendAsync("AnotherPlayerConnected", Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceivePlayerDto", JsonSerializer.Serialize<Player>(player)); //TODO: Map table to tableDto
            await Clients.Group(table.Id.ToString()).SendAsync("ReceiveTableState", JsonSerializer.Serialize<Table>(table));

            if (isNewTable)
            {
                _logger.LogInformation($"GameHub. New game started on a new table {table.Id}");
                new Thread (() => _gameProcessManager.StartRound(table.Id)).Start();
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
            _logger.LogInformation(JsonSerializer.Serialize(_allTables.First(table => table.Id == tableId)));
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Player's action received, sent to all players and added to entity.");
            //table.Mutex.ReleaseMutex();
            table.WaitForPlayerBet.Set();
            //WaitForPlayerBet.Set();
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. End");
            _logger.LogInformation($"THIS {JsonSerializer.Serialize(_allTables.First(table => table.Id == tableId))}");
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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");
            
            var (table, isPlayerRemoved) = _gameService.RemovePlayerFromTable(Context.ConnectionId);
            
            if(isPlayerRemoved) 
                _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {Context.ConnectionId} removed from table");
            

            if (table != null && isPlayerRemoved)
            {
                await Clients.Group(table.Id.ToString())
                    .SendAsync("PlayerDisconnected", JsonSerializer.Serialize<Table>(table));
                _logger.LogInformation($"GameHub.OnDisconnectedAsync. Table: {table.Id}. Players: {table.Players.Count}");

                foreach (var player in table.Players)
                {
                    _logger.LogInformation($"Player: {player.Id}, name {player.UserName}");
                }
            }
            
            if(table == null)
                _logger.LogInformation($"GameHub.OnDisconnectedAsync. Table removed");
        }
    }
}