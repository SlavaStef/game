using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Dto;

namespace PokerHand.Server.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        
        public GameHub(IGameService gameService)
        {
            _gameService = gameService;
        }
        
        public async Task ConnectToTable()
        {
            TableDto tableDto = _gameService.AddPlayerToTable(Context.ConnectionId);
            
            await Clients.Caller.SendAsync("ReceiveTable", JsonSerializer.Serialize(tableDto));
            await Clients.Others.SendAsync("AnotherPlayerEnter", "NewUserName");
        }

        public async Task Bet(int amount)
        {
            await Clients.All.SendAsync("");
        }
    }
}