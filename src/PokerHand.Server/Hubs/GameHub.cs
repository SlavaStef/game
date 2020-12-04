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
        private List<Table> _allTables;
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

        public async override Task OnConnectedAsync()
        {    
            _logger.LogInformation($"Player {Context.ConnectionId} connected");
        }

        // TODO:  Probably change to OnConnectedAsync
        public async Task ConnectToTable()
        {
            _logger.LogInformation($"Player {Context.ConnectionId} tries to connect to table");
            var (table, isNewTable, player) = _gameService.AddPlayerToTable(Context.ConnectionId, 5);
            _logger.LogInformation($"Player {Context.ConnectionId} connected to table {table.Id}");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, table.Id.ToString());
            
            await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId) // To all except connected player
                .SendAsync("AnotherPlayerConnected", Context.ConnectionId);
            //TODO: Map table to tableDto
            await Clients.Caller.SendAsync("ReceivePlayerDto", JsonSerializer.Serialize(player));
            await Clients.Group(table.Id.ToString()).SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));

            if (isNewTable)
            {
                _logger.LogInformation($"Game started on a new table {table.Id}");
                var thread = new Thread(() => _gameProcessManager.StartRound(table.Id));
                thread.Name = table.Id.ToString();
                thread.Start();
            }
        }
        
        public async void ReceivePlayerActionFromClient(string actionFromPlayer, string tableIdFromPlayer)
        {
            _logger.LogInformation($"GameHub. Method ReceivePlayerActionFromClient started");
            _logger.LogInformation($"Received from client: {actionFromPlayer}");

            var action = JsonSerializer.Deserialize<PlayerAction>(actionFromPlayer);
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdFromPlayer);
            
            _logger.LogInformation($"Action after deserialization: tableId: {tableId}, playerIndex: {action.PlayerIndexNumber}, action: {action.ActionType}");
            
            await Clients.Others.SendAsync("ReceivePlayerAction", actionFromPlayer);
            
            _logger.LogInformation("Action is sent to clients");
            
            //TODO: Optionally extract to a new method
            _allTables
                .First(table => table.Id == tableId)
                .Players
                .First(player => player.IndexNumber == action.PlayerIndexNumber)
                .CurrentAction = action;
            
            _logger.LogInformation($"Action is added to Current player");
            
            _logger.LogInformation($"GameHub. Player's action received, sent to all players and added to entity.");
            Waiter.WaitForPlayerBet.Set();
            _logger.LogInformation($"GameHub. Method ReceivePlayerActionFromClient ended");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub. Method OnDisconnectedAsync started from player {Context.ConnectionId}");
            
            var (table, isPlayerRemoved) = _gameService.RemovePlayerFromTable(Context.ConnectionId);
            
            if(isPlayerRemoved) 
                _logger.LogInformation($"GameHub. Method OnDisconnectedAsync. Player {Context.ConnectionId} removed from table");
            

            if (table != null && isPlayerRemoved)
            {
                await Clients.Group(table.Id.ToString())
                    .SendAsync("PlayerDisconnected", JsonSerializer.Serialize(table));
                _logger.LogInformation($"GameHub. Table: {table.Id}. Players: {table.Players.Count}");

                foreach (var player in table.Players)
                {
                    _logger.LogInformation($"Player: {player.Id}, name {player.UserName}");
                }
            }
            
            if(table == null)
                _logger.LogInformation($"GameHub. Table removed");
                
        }
    }
}