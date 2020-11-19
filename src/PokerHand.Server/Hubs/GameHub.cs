using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Server.Helpers;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IGameManager _gameManager;
        
        public GameHub(
            IGameService gameService,
            IGameManager gameManager)
        {
            _gameManager = gameManager;
            _gameService = gameService;
        }
        
        public async Task ConnectToTable()
        {
            var (tableDto, isNewTable) = _gameService.AddPlayerToTable(Context.ConnectionId);
            
            if (isNewTable)
                _gameManager.StartRound(tableDto.Id);
            
            await Clients.Others.SendAsync("AnotherPlayerEnter", "NewUserName");
            await Clients.All.SendAsync("ReceiveTable", JsonSerializer.Serialize(tableDto));
        }

        public async Task MakeBet(int amount)
        {
            await Clients.All.SendAsync("");
        }
    }
}