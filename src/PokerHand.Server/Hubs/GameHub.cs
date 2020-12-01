using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        
        // TODO:  Probably change to OnConnectedAsync
        public async Task ConnectToTable()
        {
            _logger.LogInformation($"Player {Context.ConnectionId} tries to connect");
            var (table, isNewTable, player) = _gameService.AddPlayerToTable(Context.ConnectionId, 5);
                                                                                                                                
            await Groups.AddToGroupAsync(Context.ConnectionId, table.Id.ToString());
            
            await Clients.GroupExcept(table.Id.ToString(), Context.ConnectionId) // To all except connected player
                .SendAsync("AnotherPlayerConnected", Context.ConnectionId);
            //TODO: Map table to tableDto
            await Clients.Caller.SendAsync("ReceivePlayerDto", JsonSerializer.Serialize(player));
            await Clients.Group(table.Id.ToString()).SendAsync("ReceiveTableState", JsonSerializer.Serialize(table));

            if (isNewTable)
            {
                _logger.LogInformation($"Game started on a new table {table.Id}");
                await _gameProcessManager.StartRound(table.Id);
            }
        }
        
        public async void ReceivePlayerActionFromClient(string actionFromPlayer, string tableIdFromPlayer)
        {
            _logger.LogInformation($"GameHub. Method ReceivePlayerActionFromClient started");
            _logger.LogInformation($"Received from client: {actionFromPlayer}");

            var action = JsonSerializer.Deserialize<PlayerAction>(actionFromPlayer);
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdFromPlayer);
            
            _logger.LogInformation($"Action after deserialization: tableId: {tableId}, playerIndex: {action.PlayerIndexNumber}, action: {action.ActionType}");
            
            await Clients.Others.SendAsync("ReceivePlayerActionFromServer", actionFromPlayer);
            
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
            _logger.LogInformation($"GameHub. Method OnDisconnectedAsync started");

            var (table, isPlayerRemoved, isTableRemoved) = _gameService.RemovePlayerFromTable(Context.ConnectionId);
            
            _logger.LogInformation($"GameHub. Method OnDisconnectedAsync. player removed");
            _logger.LogInformation($"GameHub. players: {table.Players.Count}");

            if (!isTableRemoved && isPlayerRemoved)
                await Clients.Group(table.Id.ToString())
                    .SendAsync("PlayerDisconnected", JsonSerializer.Serialize(table));
        }
    }
}