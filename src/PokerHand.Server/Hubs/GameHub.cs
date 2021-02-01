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
using PokerHand.Common.Helpers.Table;
using PokerHand.Server.Helpers;
using PokerHand.Server.Hubs.Interfaces;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub<IGameHubClient>, IGameHub
    {
        private readonly List<Table> _allTables;
        private readonly Dictionary<Guid, string> _allPlayers;
        private readonly ITableService _tableService;
        private readonly IPlayerService _playerService;
        private readonly IGameProcessManager _gameProcessManager;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            TablesCollection tablesCollection,
            PlayersOnline allPlayers,
            ITableService tableService,
            IPlayerService playerService,
            IGameProcessManager gameProcessManager,
            ILogger<GameHub> logger)
        {
            _allTables = tablesCollection.Tables;
            _allPlayers = allPlayers.Players;
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
            var playerProfileDto = await _playerService.AddNewPlayer(userName);

            if (playerProfileDto == null)
                return;
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(playerProfileDto));

            _allPlayers.Add(playerProfileDto.Id, Context.ConnectionId);
            
            _logger.LogInformation($"Player {playerProfileDto.UserName} is registered");
        } 

        public async Task Authenticate(string playerId)
        {
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            
            var playerProfileDto = await _playerService.Authenticate(playerIdGuid);

            if (playerProfileDto == null)
            {
                await Clients.Caller.ReceivePlayerNotFound();
                return;
            }

            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(playerProfileDto));

            if (_allPlayers.ContainsKey(playerIdGuid))
                _allPlayers[playerIdGuid] = Context.ConnectionId;
            else
                _allPlayers.Add(playerIdGuid, Context.ConnectionId);
            
            _logger.LogInformation($"Player {playerProfileDto.UserName} is authenticated");
        }

        public async Task GetTableInfo(string tableTitle)
        {
            var tableInfo = _tableService.GetTableInfo(tableTitle);
            
            await Clients.Caller
                .ReceiveTableInfo(JsonSerializer.Serialize(tableInfo));
        }
        
        public async Task GetAllTablesInfo()
        {
            var allTablesInfo = _tableService.GetAllTablesInfo();
            
            await Clients.Caller
                .ReceiveAllTablesInfo(JsonSerializer.Serialize(allTablesInfo));
        }
        
        public async Task ConnectToTable(string tableTitle, string playerId, string buyInAmount, string autoTop)
        {
            var connectOptions = new TableConnectionOptions 
            {
                TableTitle = JsonSerializer.Deserialize<TableTitle>(tableTitle),
                PlayerId = JsonSerializer.Deserialize<Guid>(playerId),
                PlayerConnectionId = Context.ConnectionId,
                BuyInAmount = JsonSerializer.Deserialize<int>(buyInAmount),
                IsAutoTop = JsonSerializer.Deserialize<bool>(autoTop)
            };
            
            var (tableDto, playerDto, isNewTable) = await _tableService.AddPlayerToTable(connectOptions);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, tableDto.Id.ToString());
            await Clients.Caller
                .ReceivePlayerDto(JsonSerializer.Serialize(playerDto));
            await Clients.Group(tableDto.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(tableDto));

            if (isNewTable)
            {
                if (tableDto.Type == TableType.SitAndGo)
                    new Thread (() => _gameProcessManager.StartSitAndGoRound(tableDto.Id)).Start();
                else
                    new Thread (() => _gameProcessManager.StartRound(tableDto.Id)).Start();
            }
        }
        
        public async Task ReceivePlayerActionFromClient(string actionFromPlayer, string tableIdFromPlayer)
        {
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Start");
            
            var action = JsonSerializer.Deserialize<PlayerAction>(actionFromPlayer);
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdFromPlayer);
            
            await Clients.OthersInGroup(tableId.ToString())
                .ReceivePlayerAction(actionFromPlayer);
            
            var table = _allTables.First(t => t.Id == tableId);
            
            table.ActivePlayers
                .First(p => p.IndexNumber == action?.PlayerIndexNumber)
                .CurrentAction = action;
            
            table.WaitForPlayerBet.Set();
        }

        public async Task SendPlayerProfile(string playerId)
        {
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);

            _allPlayers.TryGetValue(playerIdGuid, out var connectionId);

            if (connectionId != Context.ConnectionId)
                return;
            
            var profileDto = await _playerService.GetPlayerProfile(playerIdGuid);
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(profileDto));
        }

        public void ReceiveActivePlayerStatus(string tableId, string playerId)
        {
            var tableIdGuid = Guid.Parse(tableId);
            var playerIdGuid = Guid.Parse(playerId);

            _playerService.SetPlayerReady(tableIdGuid, playerIdGuid);
        }

        public async Task AddStackMoney(string tableId, string playerId, string amount)
        {
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            var amountInt = JsonSerializer.Deserialize<int>(amount);

            await _playerService.AddStackMoneyFromTotalMoney(tableIdGuid, playerIdGuid, amountInt);
        }

        public async Task LeaveTable(string tableId, string playerId)
        {
            
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);

            var tableDto = await _tableService.RemovePlayerFromTable(tableIdGuid, playerIdGuid);

            if (tableDto != null)
                await Clients.GroupExcept(tableId, Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(tableDto));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");
            WriteAllPlayersList();
            
            var playerId = _allPlayers.First(p => p.Value == Context.ConnectionId).Key;
            var table = _allTables
                .FirstOrDefault(t => t.Players.FirstOrDefault(p => p.Id == playerId) != null);
            
            if (table != null)
            {
                if (table.Players.Count == 1)
                    await _tableService.RemovePlayerFromTable(table.Id, playerId);
                else
                {
                    var newTableDto = await _tableService.RemovePlayerFromTable(table.Id, playerId);
                    await Clients.Group(table.Id.ToString())
                        .PlayerDisconnected(JsonSerializer.Serialize(newTableDto));
                }
            }
            
            _allPlayers.Remove(playerId);
            
            WriteAllPlayersList();
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {playerId} : {Context.ConnectionId} removed from table");
        }

        private void WriteAllPlayersList() =>
            _logger.LogInformation($"AllPlayers: {JsonSerializer.Serialize(_allPlayers)}");

    }
}