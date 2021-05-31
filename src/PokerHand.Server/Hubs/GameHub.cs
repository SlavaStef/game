using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly IPresentService _presentService;
        private readonly IMapper _mapper;
        private readonly IChatService _chatService;

        public GameHub(
            ITableService tableService,
            IPlayerService playerService,
            IPlayersOnline allPlayers, 
            ITablesOnline allTables, 
            IMapper mapper, 
            IGameProcessService gameProcessService, 
            IPresentService presentService,
            IChatService chatService)
        {
            _tableService = tableService;
            _playerService = playerService;
            _allPlayers = allPlayers;
            _allTables = allTables;
            _mapper = mapper;
            _gameProcessService = gameProcessService;
            _presentService = presentService;
            _chatService = chatService;
        
            RegisterEventHandlers();
        }
        
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller
                .ReceiveConnectionId(Context.ConnectionId);
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

        public async Task ReceiveNewBuyIn(string tableIdJson, string playerIdJson, string newBuyInJson, string isAutoTopJson)
        {
            var tableId = JsonSerializer.Deserialize<Guid>(tableIdJson);
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            var newBuyIn = JsonSerializer.Deserialize<int>(newBuyInJson);
            var isAutoTop = JsonSerializer.Deserialize<bool>(isAutoTopJson);

            await _playerService.AddStackMoneyFromTotalMoney(tableId, playerId, newBuyIn);

            _playerService.ChangeAutoTop(tableId, playerId, newBuyIn, isAutoTop);
        }

        public async Task ShowWinnerCards(string tableIdString)
        {
            await Clients.GroupExcept(tableIdString, Context.ConnectionId)
                .ShowWinnerCards();
        }
    }
}