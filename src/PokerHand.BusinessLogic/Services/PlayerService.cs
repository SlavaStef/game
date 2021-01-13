using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;
        

        public PlayerService(UserManager<Player> userManager, IMapper mapper, ILogger<TableService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PlayerProfileDto> AddNewPlayer(string playerName, ILogger logger)
        {
            logger.LogInformation("AddNewPlayer. Start");
            var newPlayer = new Player
            {
                UserName = playerName,
                Country = "",
                RegistrationDate = DateTime.Now,
                Experience = 0,
                TotalMoney = 10000,
                CoinsAmount = 0,
                GamesPlayed = 0,
                BestHandType = HandType.None,
                GamesWon = 0,
                BiggestWin = 0,
                SitAndGoWins = 0
            };
            
            logger.LogInformation($"AddNewPlayer. Player: {JsonSerializer.Serialize(newPlayer)}");

            var identityResult = await _userManager.CreateAsync(newPlayer);
            logger.LogInformation($"AddNewPlayer. Result: {JsonSerializer.Serialize(identityResult)}");
            
            if (!identityResult.Succeeded)
                throw new Exception();
            logger.LogInformation($"AddNewPlayer. End");
            return _mapper.Map<PlayerProfileDto>(newPlayer);
        }

        public async Task<PlayerProfileDto> Authenticate(Guid playerId)
        {
            _logger.LogInformation("PlayerService.Authenticate. Start");
            var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);

            _logger.LogInformation($"PlayerService.Authenticate. Player: {JsonSerializer.Serialize(player)}");
            
            _logger.LogInformation("PlayerService.Authenticate. End");
            return player == null 
                ? null 
                : _mapper.Map<PlayerProfileDto>(player);
        }
    }
}