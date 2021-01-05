﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Server.Helpers;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub, IGameHub
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
            var playerProfileDto = await _playerService.AddNewPlayer(userName, _logger);
            
            await Clients.Caller.SendAsync("ReceivePlayerProfile", JsonSerializer.Serialize(playerProfileDto));
            
            _allPlayers.Add(playerProfileDto.Id, Context.ConnectionId);
            _logger.LogInformation("RegisterNewPlayer. End");
        } 

        public async Task Authenticate(string playerId)
        {
            _logger.LogInformation("Authenticate. Start");
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            
            _logger.LogInformation($"Authenticate. playerId:{playerId}, GUID: {playerIdGuid}");
            
            var playerProfileDto = await _playerService.Authenticate(playerIdGuid);
            
            try
            { 
                _allPlayers.Add(playerIdGuid, Context.ConnectionId);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Authenticate. Exception: {e.Message}");
            }

            if (playerProfileDto == null)
                await Clients.Caller.SendAsync("ReceivePlayerNotFound");
            else
                await Clients.Caller.SendAsync("ReceivePlayerProfile", JsonSerializer.Serialize(playerProfileDto));
        }

        public async Task GetTableInfo(string tableTitle)
        {
            var tableInfo = _tableService.GetTableInfo(tableTitle);
            await Clients.Caller.SendAsync("ReceiveTableInfo", JsonSerializer.Serialize(tableInfo));
        }
        
        public async Task GetAllTablesInfo()
        {
            var allTablesInfo = _tableService.GetAllTablesInfo();
            await Clients.Caller.SendAsync("ReceiveAllTablesInfo", JsonSerializer.Serialize(allTablesInfo));
        }
        
        public async Task ConnectToTable(string tableTitle, string playerId, string buyInAmount)
        {
            var title = JsonSerializer.Deserialize<TableTitle>(tableTitle);
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);
            var buyIn = JsonSerializer.Deserialize<int>(buyInAmount);
            
            var (tableDto, isNewTable, playerDto) = await _tableService.AddPlayerToTable(title, playerIdGuid, Context.ConnectionId, buyIn);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, tableDto.Id.ToString());
            await Clients.Caller.SendAsync("ReceivePlayerDto", JsonSerializer.Serialize(playerDto));
            await Clients.Group(tableDto.Id.ToString()).SendAsync("ReceiveTableState", JsonSerializer.Serialize(tableDto));

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
            table.WaitForPlayerBet.Set();
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
            _logger.LogInformation($"GameHub.LeaveTable. Start");
            var tableIdGuid = Guid.Parse(tableId);
            var playerIdGuid = Guid.Parse(playerId);
            
            _logger.LogInformation($"GameHub.LeaveTable. tableIdGuid: {tableIdGuid}, playerIdGuid: {playerIdGuid}");

            var tableDto = await _tableService.RemovePlayerFromTable(tableIdGuid, playerIdGuid);
            
            _logger.LogInformation($"GameHub.LeaveTable. tableDto: {JsonSerializer.Serialize(tableDto)}");

            if (tableDto != null)
                await Clients.Group(tableId).SendAsync("PlayerDisconnected", JsonSerializer.Serialize(tableDto));
            
            _logger.LogInformation($"GameHub.LeaveTable. End");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Start from player {Context.ConnectionId}");

            var playerId = _allPlayers.First(p => p.Value == Context.ConnectionId).Key;
            var tableId = _allTables.First(t => t.Players.First(p => p.Id == playerId) != null).Id;
            
            var newTableDto = await _tableService.RemovePlayerFromTable(tableId, playerId);

            _allPlayers.Remove(playerId);
            
            _logger.LogInformation($"GameHub.OnDisconnectedAsync. Player {Context.ConnectionId} removed from table");
            
            await Clients.Group(tableId.ToString())
                .SendAsync("PlayerDisconnected", JsonSerializer.Serialize(newTableDto));
        
        }
    }
}