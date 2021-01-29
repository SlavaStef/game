﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public GameHub(
            TablesCollection tablesCollection,
            ITableService tableService,
            IPlayerService playerService,
            IGameProcessManager gameProcessManager,
            ILogger<GameHub> logger, 
            IMapper mapper, 
            PlayersOnline allPlayers)
        {
            _allTables = tablesCollection.Tables;
            _tableService = tableService;
            _playerService = playerService;
            _gameProcessManager = gameProcessManager;
            _logger = logger;
            _mapper = mapper;
            _allPlayers = allPlayers.Players;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"GameHub. Player {Context.ConnectionId} connected");
        }
        
        public async Task RegisterNewPlayer(string userName)
        {
            _logger.LogInformation("RegisterNewPlayer. Start");
            WriteAllPlayersList();
            var playerProfileDto = await _playerService.AddNewPlayer(userName, _logger);

            await Clients.Caller.ReceivePlayerProfile(JsonSerializer.Serialize(playerProfileDto));

            try
            {
                _allPlayers.Add(playerProfileDto.Id, Context.ConnectionId);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"{e.Message}");
                _logger.LogInformation($"{e.StackTrace}");
                throw;
            }
            
            WriteAllPlayersList();
            _logger.LogInformation("RegisterNewPlayer. End");
        } 

        public async Task Authenticate(string playerId)
        {
            _logger.LogInformation("Authenticate. Start");
            WriteAllPlayersList();
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            
            _logger.LogInformation($"Authenticate. playerId:{playerId}, GUID: {playerIdGuid}");
            
            var playerProfileDto = await _playerService.Authenticate(playerIdGuid);

            if (playerProfileDto == null)
                await Clients.Caller.ReceivePlayerNotFound();
            else
            {
                try
                {
                    _allPlayers.Add(playerIdGuid, Context.ConnectionId);
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{e.Message}");
                    _logger.LogInformation($"{e.StackTrace}");
                    throw;
                }
                
                WriteAllPlayersList();
                await Clients.Caller
                    .ReceivePlayerProfile(JsonSerializer.Serialize(playerProfileDto));
            }
        }

        public async Task GetTableInfo(string tableTitle)
        {
            var tableInfo = _tableService.GetTableInfo(tableTitle);
            await Clients.Caller.ReceiveTableInfo(JsonSerializer.Serialize(tableInfo));
        }
        
        public async Task GetAllTablesInfo()
        {
            var allTablesInfo = _tableService.GetAllTablesInfo();
            await Clients.Caller.ReceiveAllTablesInfo(JsonSerializer.Serialize(allTablesInfo));
        }
        
        public async Task ConnectToTable(string tableTitle, string playerId, string buyInAmount, string autoTop)
        {
            WriteAllPlayersList();
            
            var title = JsonSerializer.Deserialize<TableTitle>(tableTitle);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            var buyIn = JsonSerializer.Deserialize<int>(buyInAmount);
            var isAutoTop = JsonSerializer.Deserialize<bool>(autoTop);
            
            _logger.LogInformation($"tableTitle: {JsonSerializer.Serialize(tableTitle)}");
            _logger.LogInformation($"playerId: {JsonSerializer.Serialize(playerId)}");
            _logger.LogInformation($"buyInAmount: {JsonSerializer.Serialize(buyInAmount)}");
            _logger.LogInformation($"autoTop: {JsonSerializer.Serialize(autoTop)}");
            _logger.LogInformation($"Context.ConnectionId: {JsonSerializer.Serialize(Context.ConnectionId)}");
            
            var (tableDto, isNewTable, playerDto) = await _tableService.AddPlayerToTable(title, playerIdGuid, Context.ConnectionId, buyIn, isAutoTop);
            
            _logger.LogInformation($"TableDto: {tableDto}");
            _logger.LogInformation($"TableDto: {tableDto.Id}");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, tableDto.Id.ToString());
            await Clients.Caller
                .ReceivePlayerDto(JsonSerializer.Serialize(playerDto));
            await Clients.Group(tableDto.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(tableDto));

            if (isNewTable)
            {
                if(tableDto.Type == TableType.SitAndGo)
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
            
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Action is added to Current player");
            _logger.LogInformation(JsonSerializer.Serialize(_allTables.First(t => t.Id == tableId)));
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Player's action received, sent to all players and added to entity.");
            table.WaitForPlayerBet.Set();
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. End");
            _logger.LogInformation($"THIS {JsonSerializer.Serialize(_allTables.First(t => t.Id == tableId))}");
        }

        public async Task SendPlayerProfile(string playerId)
        {
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            var profileDto = await _playerService.GetPlayerProfile(playerIdGuid);
            
            await Clients.Caller.ReceivePlayerProfile(JsonSerializer.Serialize(profileDto));
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

        public async Task AddStackMoney(string tableId, string playerId, string amount)
        {
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            var amountInt = JsonSerializer.Deserialize<int>(amount);

            if (await _playerService.GetStackMoney(playerIdGuid, amountInt))
                _allTables.First(t => t.Id == tableIdGuid).Players.Find(p => p.Id == playerIdGuid).StackMoney =
                    amountInt;
        }

        public async Task LeaveTable(string tableId, string playerId)
        {
            _logger.LogInformation($"GameHub.LeaveTable. Start");
            WriteAllPlayersList();
            var tableIdGuid = JsonSerializer.Deserialize<Guid>(tableId);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            
            _logger.LogInformation($"GameHub.LeaveTable. tableIdGuid: {tableIdGuid}, playerIdGuid: {playerIdGuid}");

            var tableDto = await _tableService.RemovePlayerFromTable(tableIdGuid, playerIdGuid);
            
            _logger.LogInformation($"GameHub.LeaveTable. tableDto: {JsonSerializer.Serialize(tableDto)}");

            if (tableDto != null)
                await Clients.GroupExcept(tableId, Context.ConnectionId)
                    .PlayerDisconnected(JsonSerializer.Serialize(tableDto));
            
            _logger.LogInformation($"GameHub.LeaveTable. End");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");

            WriteAllPlayersList();
            
            var playerId = _allPlayers.First(p => p.Value == Context.ConnectionId).Key;

            if (_allTables.FirstOrDefault(t => t.Players.FirstOrDefault(p => p.Id == playerId) != null) != null)
            {
                var table = _allTables.First(t => t.Players.First(p => p.Id == playerId) != null);
                
                if (table.Players.Count == 1)
                {
                    await _tableService.RemovePlayerFromTable(table.Id, playerId);
                }
                else
                {
                    var newTableDto = await _tableService.RemovePlayerFromTable(table.Id, playerId);
                    await Clients.Group(table.Id.ToString()).PlayerDisconnected(JsonSerializer.Serialize(newTableDto));
                }
            }
            
            _allPlayers.Remove(playerId);
            
            try
            {
                _allPlayers.Remove(playerId);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"{e.Message}");
                _logger.LogInformation($"{e.StackTrace}");
                throw;
            }
            
            WriteAllPlayersList();
            
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {Context.ConnectionId} removed from table");
        }

        private void WriteAllPlayersList() =>
            _logger.LogInformation($"AllPlayers: {JsonSerializer.Serialize(_allPlayers)}");

    }
}