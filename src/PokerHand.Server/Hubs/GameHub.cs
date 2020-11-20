using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Server.Helpers;
using Serilog;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IGameManager _gameManager;
        private readonly ILogger<GameHub> _logger;
        
        public GameHub(
            IGameService gameService,
            IGameManager gameManager,
            ILogger<GameHub> logger)
        {
            _gameManager = gameManager;
            _gameService = gameService;
            _logger = logger;
        }
        
        public async Task ConnectToTable()
        {
            _logger.LogInformation("New player connected to game");
            // var (tableDto, isNewTable) = _gameService.AddPlayerToTable(Context.ConnectionId);
            //
            // if (isNewTable)
            //     _gameManager.StartRound(tableDto.Id);
            
            await Clients.Others.SendAsync("AnotherPlayerEnter", "NewUserName");
            await Clients.All.SendAsync("ReceiveTable", JsonSerializer.Serialize("tableDto"));
        }
        

        public async Task MakeBet(int amount)
        {
            await Clients.All.SendAsync("");
        }
    }
}