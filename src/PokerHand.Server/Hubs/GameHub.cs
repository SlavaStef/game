using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Helpers.GameProcess;
using PokerHand.Common.Helpers.Table;
using PokerHand.Server.Hubs.Interfaces;
using Serilog;

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
        private readonly ILoginService _loginService;
        private readonly IPresentService _presentService;
        private readonly IMapper _mapper;

        public GameHub(
            ITableService tableService,
            IPlayerService playerService,
            IPlayersOnline allPlayers, 
            ITablesOnline allTables, 
            IMapper mapper, 
            IMediaService mediaService, 
            IGameProcessService gameProcessService, 
            ILoginService loginService, 
            IPresentService presentService)
        {
            _tableService = tableService;
            _playerService = playerService;
            _allPlayers = allPlayers;
            _allTables = allTables;
            _mapper = mapper;
            _mediaService = mediaService;
            _gameProcessService = gameProcessService;
            _loginService = loginService;
            _presentService = presentService;

            RegisterEventHandlers();
        }

        public async Task Test(string message)
        {
            await Clients.Caller.ReceiveTestMessage(message);
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
        
        //TODO: should receive one object
        public async Task ConnectToTable(string tableTitleJson, string playerIdJson, string buyInJson, string isAutoTopJson)
        {
            var connectionOptions = new TableConnectionOptions
            {
                TableTitle = JsonSerializer.Deserialize<TableTitle>(tableTitleJson),
                PlayerId = JsonSerializer.Deserialize<Guid>(playerIdJson),
                PlayerConnectionId = Context.ConnectionId,
                BuyInAmount = JsonSerializer.Deserialize<int>(buyInJson),
                IsAutoTop = JsonSerializer.Deserialize<bool>(isAutoTopJson)
            };

            var validationResult = await new TableConnectionOptionsValidator().ValidateAsync(connectionOptions);
            if (validationResult.IsValid is not true)
            {
                foreach (var error in validationResult.Errors)
                    Log.Error($"{error.ErrorMessage}");

                return;
            }

            var connectionResult = await _tableService.AddPlayerToTable(connectionOptions);
            if (connectionResult.IsSuccess is false)
            {
                Log.Error($"ConnectToTable Error: {connectionResult.Message}");
                return;
            }
            
            await AddPlayerToTableGroup(connectionResult);

            if (connectionResult.Value.IsNewTable is true) 
                StartGameOnNewTable(connectionResult.Value.TableDto.Id);
        }

        public void ReceivePlayerActionFromClient(string actionJson, string tableIdJson)
        {
            Log.Information($"GameHub.ReceivePlayerActionFromClient. Start by {Context.ConnectionId}");
            
            var action = JsonSerializer.Deserialize<PlayerAction>(actionJson);
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);

            var table = _allTables.GetById(tableId);
            
            table.ActivePlayers
                .First(p => p.IndexNumber == action?.PlayerIndexNumber)
                .CurrentAction = action;
            
            table.WaitForPlayerBet.Set();
        }
        
        //TODO: change to Json
        public void ReceiveActivePlayerStatus(string tableId, string playerId)
        {
            var tableIdGuid = Guid.Parse(tableId);
            var playerIdGuid = Guid.Parse(playerId);

            _playerService.SetPlayerReady(tableIdGuid, playerIdGuid);
        }

        public async Task ReceiveNewBuyIn(string tableIdJson, string playerIdJson, string amountJson, string isAutoTopJson)
        {
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            var amount = JsonSerializer.Deserialize<int>(amountJson);
            var isAutoTop = JsonSerializer.Deserialize<bool>(isAutoTopJson);

            await _playerService.AddStackMoneyFromTotalMoney(tableId, playerId, amount);

            _playerService.ChangeAutoTop(tableId, playerId, isAutoTop);
        }
    }
}