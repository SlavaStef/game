using System;
using System.Linq;
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
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.BusinessLogic.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;
        private readonly IPlayerRepository _playerRepository;
        

        public PlayerService(
            UserManager<Player> userManager, 
            IMapper mapper, 
            ILogger<TableService> logger, 
            IPlayerRepository playerRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _playerRepository = playerRepository;
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

        public async Task<bool> AddTotalMoney(Guid playerId, int amountToAdd)
        {
            var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            player.TotalMoney += amountToAdd;
            
            var identityResult = await _userManager.UpdateAsync(player);
            //await _playerRepository.SaveChangesAsync();

            return identityResult.Succeeded;
        }

        public async Task<int> GetStackMoney(Guid playerId, int requiredAmount)
        {
            var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            if (player.TotalMoney < requiredAmount) 
                return 0;

            await _playerRepository.SubtractFromTotalMoney(playerId, requiredAmount);
            //player.TotalMoney -= requiredAmount;

            //await _userManager.UpdateAsync(player);
            //await _playerRepository.SaveChangesAsync();
            
            return requiredAmount;
        }

        public async Task<bool> ReturnToTotalMoney(Guid playerId, int amountToAdd)
        {
            _logger.LogInformation("PlayerService.ReturnToTotalMoney. Start");

            //var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            //player.TotalMoney += amountToAdd;
            
            try
            {
                //await _userManager.UpdateAsync(player);

                await _playerRepository.AddToTotalMoney(playerId, amountToAdd);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"PlayerService.ReturnToTotalMoney. {e.Message}");
                _logger.LogInformation($"PlayerService.ReturnToTotalMoney. {e.InnerException}");
                _logger.LogInformation($"PlayerService.ReturnToTotalMoney. {e.StackTrace}");
                _logger.LogInformation($"PlayerService.ReturnToTotalMoney. {e.Source}");
                throw;
            }
            
            //await _playerRepository.SaveChangesAsync();
            //_logger.LogInformation($"PlayerService.ReturnToTotalMoney. User after: {JsonSerializer.Serialize(await _userManager.Users.FirstOrDefaultAsync(p => p.Id == player.Id))}");
            _logger.LogInformation("PlayerService.ReturnToTotalMoney. End");
            return true;
        }
    }
}