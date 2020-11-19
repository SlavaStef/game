using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Server.Hubs;

namespace PokerHand.Server.Helpers
{
    public interface IGameManager
    {
        public void StartRound(Guid tableId);
    }
    
    public class GameManager : IGameManager
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly IGameService _gameService;

        public GameManager(
            IGameService gameService,
            IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
            _gameService = gameService;
        }

        public void StartRound(Guid tableId)
        {
            Task.Run(() => _gameService.StartRound(tableId));
        }
    }
}