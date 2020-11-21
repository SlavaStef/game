using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;
using PokerHand.Server.Helpers;
using Serilog;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IGameProcessManager _gameProcessManager;
        private readonly ILogger<GameHub> _logger;
        
        public GameHub(
            IGameService gameService,
            IGameProcessManager gameProcessManager,
            ILogger<GameHub> logger)
        {
            _gameProcessManager = gameProcessManager;
            _gameService = gameService;
            _logger = logger;
        }
        
        // TODO:  Probably change to OnConnectedAsync
        public async Task ConnectToTable()
        {
            _logger.LogInformation($"Player {Context.ConnectionId} try to connect");
            var (tableDto, isNewTable) = _gameService.AddPlayerToTable(Context.ConnectionId);
                                                                                                                                
            await Groups.AddToGroupAsync(Context.ConnectionId, tableDto.Id.ToString());
            
            await Clients.GroupExcept(tableDto.Id.ToString(), Context.ConnectionId).SendAsync("AnotherPlayerEnter", Context.ConnectionId.ToString());
            await Clients.Group(tableDto.Id.ToString()).SendAsync("ReceiveTable", JsonSerializer.Serialize(tableDto));

            if (isNewTable)
            {
                _logger.LogInformation($"Game started on a new table {tableDto.Id}");
                await _gameProcessManager.StartRound(tableDto.Id);
            }
        }

        public async void ReceivePlayerAction(PlayerAction action)
        {
            await Clients.Group(action.TableId.ToString()).SendAsync("ReceivePlayerAction", action);
            Waiter.waitForPlayerBet.Set();
        }
    }
}