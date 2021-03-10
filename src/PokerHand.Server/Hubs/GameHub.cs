using System;
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
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Table;
using PokerHand.Server.Helpers;
using PokerHand.Server.Hubs.Interfaces;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub : Hub<IGameHubClient>, IGameHub
    {
        private readonly ITablesOnline _allTables;
        private readonly IPlayersOnline _allPlayers;
        private readonly ITableService _tableService;
        private readonly IPlayerService _playerService;
        private readonly IGameProcessManager _gameProcessManager;
        private readonly ILogger<GameHub> _logger;
        private readonly IMapper _mapper;

        public GameHub(
            ITableService tableService,
            IPlayerService playerService,
            IGameProcessManager gameProcessManager,
            ILogger<GameHub> logger, 
            IPlayersOnline allPlayers, 
            ITablesOnline allTables, 
            IMapper mapper)
        {
            _tableService = tableService;
            _playerService = playerService;
            _gameProcessManager = gameProcessManager;
            _logger = logger;
            _allPlayers = allPlayers;
            _allTables = allTables;
            _mapper = mapper;
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

            _allPlayers.AddOrUpdate(playerIdGuid, Context.ConnectionId);
            
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
                new Thread (() => _gameProcessManager.StartRound(tableDto.Id)).Start();
            }
        }
        
        public async Task ReceivePlayerActionFromClient(string actionFromPlayer, string tableIdFromPlayer)
        {
            try
            {
                _logger.LogInformation($"GameHub.ReceivePlayerActionFromClient. Start by {Context.ConnectionId}");
            
                var action = JsonSerializer.Deserialize<PlayerAction>(actionFromPlayer);
                var tableId = JsonSerializer.Deserialize<Guid>(tableIdFromPlayer);

                var table = _allTables.GetById(tableId);
            
                table.ActivePlayers
                    .First(p => p.IndexNumber == action?.PlayerIndexNumber)
                    .CurrentAction = action;
            
                table.WaitForPlayerBet.Set();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}");
                _logger.LogError($"{e.StackTrace}");
                throw;
            }
            
        }

        public async Task SendPlayerProfile(string playerId)
        {
            var playerIdGuid = JsonSerializer.Deserialize<Guid>(playerId);

            var connectionId = _allPlayers.GetValueByKey(playerIdGuid);

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

        private void WriteAllPlayersList() =>
            _logger.LogInformation($"AllPlayers: {JsonSerializer.Serialize(_allPlayers.GetAll())}");

    }
}