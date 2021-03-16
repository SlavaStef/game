﻿using System;
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
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;
using PokerHand.Server.Hubs.Interfaces;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub : Hub<IGameHubClient>, IGameHub
    {
        private readonly IGameProcessService _gameProcessService;
        private readonly ITablesOnline _allTables;
        private readonly IPlayersOnline _allPlayers;
        private readonly ITableService _tableService;
        private readonly IPlayerService _playerService;
        private readonly IMediaService _mediaService;
        private readonly ILogger<GameHub> _logger;
        private readonly IMapper _mapper;

        public GameHub(
            ITableService tableService,
            IPlayerService playerService,
            ILogger<GameHub> logger, 
            IPlayersOnline allPlayers, 
            ITablesOnline allTables, 
            IMapper mapper, 
            IMediaService mediaService, 
            IGameProcessService gameProcessService)
        {
            _tableService = tableService;
            _playerService = playerService;
            _logger = logger;
            _allPlayers = allPlayers;
            _allTables = allTables;
            _mapper = mapper;
            _mediaService = mediaService;
            _gameProcessService = gameProcessService;

            RegisterEventHandlers();
        }
        
        //TODO: change to Json
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
        
        //TODO: change to Json
        //TODO: should receive one object
        public async Task ConnectToTable(string tableTitleJson, string playerIdJson, string buyInAmountJson, string isAutoTopJson)
        {
            var connectOptions = new TableConnectionOptions 
            {
                TableTitle = JsonSerializer.Deserialize<TableTitle>(tableTitleJson),
                PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson),
                PlayerConnectionId = Context.ConnectionId,
                BuyInAmount = JsonSerializer.Deserialize<int>(buyInAmountJson),
                IsAutoTop = JsonSerializer.Deserialize<bool>(isAutoTopJson)
            };
            
            var connectionResult = await _tableService.AddPlayerToTable(connectOptions);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, connectionResult.TableDto.Id.ToString());
            await Clients.Caller
                .ReceivePlayerDto(JsonSerializer.Serialize(connectionResult.PlayerDto));
            await Clients.Group(connectionResult.TableDto.Id.ToString())
                .ReceiveTableState(JsonSerializer.Serialize(connectionResult.TableDto));

            if (connectionResult.IsNewTable)
                new Thread (() => _gameProcessService.StartRound(connectionResult.TableDto.Id)).Start();
        }
        
        public void ReceivePlayerActionFromClient(string actionJson, string tableIdJson)
        {
            _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Start by {Context.ConnectionId}");
            
            var action = JsonSerializer.Deserialize<PlayerAction>(actionJson);
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);

            var table = _allTables.GetById(tableId);
            
            table.ActivePlayers
                .First(p => p.IndexNumber == action?.PlayerIndexNumber)
                .CurrentAction = action;
            
            table.WaitForPlayerBet.Set();
        }

        public async Task SendPlayerProfile(string playerIdJson)
        {
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);

            var connectionId = _allPlayers.GetValueByKey(playerId);

            if (connectionId != Context.ConnectionId)
                return;
            
            var profileDto = await _playerService.GetPlayerProfile(playerId);
            
            await Clients.Caller
                .ReceivePlayerProfile(JsonSerializer.Serialize(profileDto));
        }

        //TODO: change to Json
        public void ReceiveActivePlayerStatus(string tableId, string playerId)
        {
            var tableIdGuid = Guid.Parse(tableId);
            var playerIdGuid = Guid.Parse(playerId);

            _playerService.SetPlayerReady(tableIdGuid, playerIdGuid);
        }

        public async Task AddStackMoney(string tableIdJson, string playerIdJson, string amountJson)
        {
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            var amount = JsonSerializer.Deserialize<int>(amountJson);

            await _playerService.AddStackMoneyFromTotalMoney(tableId, playerId, amount);
        }

        private void WriteAllPlayersList() =>
            _logger.LogInformation($"AllPlayers: {JsonSerializer.Serialize(_allPlayers.GetAll())}");
        
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
                await Clients.GroupExcept(table.Id.ToString(), table.CurrentPlayer.ConnectionId)
                    .ReceivePlayerAction(action);
            };

            _gameProcessService.ReceiveCurrentPlayerIdInWagering += async (table, currentPlayerIdJson) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceiveCurrentPlayerIdInWagering(currentPlayerIdJson);
            };
            
                _gameProcessService.BigBlindBetEvent += async (table, bigBlindAction) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceivePlayerAction(JsonSerializer.Serialize(bigBlindAction));
            };
            
            _gameProcessService.SmallBlindBetEvent += async (table, smallBlindAction) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceivePlayerAction(JsonSerializer.Serialize(smallBlindAction));
            };
            
            _gameProcessService.NewPlayerBetEvent += async (table, newPlayerBlindAction) =>
            {
                await Clients.Group(table.Id.ToString())
                    .ReceivePlayerAction(JsonSerializer.Serialize(newPlayerBlindAction));
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

            _gameProcessService.RemoveFromGroupAsync += async (connectionId, groupName) =>
            {
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
        }
    }
}