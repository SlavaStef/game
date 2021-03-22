using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.Player;
using PokerHand.DataAccess.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PokerHand.BusinessLogic.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IMediaService _mediaService;
        private readonly ITablesOnline _allTables;
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<PlayerService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public PlayerService(
            UserManager<Player> userManager, 
            IMapper mapper, 
            ILogger<PlayerService> logger, 
            IUnitOfWork unitOfWork, 
            ITablesOnline allTables, 
            IMediaService mediaService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _allTables = allTables;
            _mediaService = mediaService;
        }

        public async Task<PlayerProfileDto> CreatePlayer(string playerName)
        {
            var newPlayer = new Player
            {
                Type = PlayerType.Human,
                UserName = playerName,
                Country = "",
                RegistrationDate = DateTime.Now,
                Experience = 0,
                TotalMoney = 100000000,
                CoinsAmount = 0,
                GamesPlayed = 0,
                BestHandType = HandType.None,
                GamesWon = 0,
                BiggestWin = 0,
                SitAndGoWins = 0
            };
            
            var createResult = await _userManager.CreateAsync(newPlayer);

            if (!createResult.Succeeded)
                return null;

            var setImageResult = await _mediaService.SetDefaultProfileImage(JsonSerializer.Serialize(newPlayer.Id));
            if (setImageResult.IsSuccess is false)
            {
                _logger.LogError($"CreatePlayer. {setImageResult.Message} playerId: {newPlayer.Id}");
            }
            
            _logger.LogInformation($"New player {newPlayer.UserName} registered");
            return _mapper.Map<PlayerProfileDto>(newPlayer);
        }

        public async Task<PlayerProfileDto> Authenticate(Guid playerId)
        {
            var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);

            _logger.LogInformation($"Authenticate result: {player}");
            return player == null 
                ? null
                : _mapper.Map<PlayerProfileDto>(player);
        }
        
        public async Task<bool> GetFromTotalMoney(Guid playerId, int amount)
        {
            var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            if (player.TotalMoney < amount) 
                return false;

            await _unitOfWork.Players.SubtractTotalMoneyAsync(playerId, amount);
            
            return true;
        }

        public async Task AddTotalMoney(Guid playerId, int amountToAdd)
        {
            await _unitOfWork.Players.AddTotalMoneyAsync(playerId, amountToAdd);
        }

        public async Task<bool> AddStackMoneyFromTotalMoney(Guid tableId, Guid playerId, int requiredAmount)
        {
            var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            if (player.TotalMoney < requiredAmount) 
                return false;

            await _unitOfWork.Players.SubtractTotalMoneyAsync(playerId, requiredAmount);

            _allTables.GetById(tableId)
                .Players
                .First(p => p.Id == playerId)
                .StackMoney = requiredAmount;
            
            return true;
        }

        public async Task<PlayerProfileDto> GetPlayerProfile(Guid playerId)
        {
            var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);

            return player == null 
                ? null 
                : _mapper.Map<PlayerProfileDto>(player);
        }
        
        public void SetPlayerReady(Guid tableId, Guid playerId)
        {
            _allTables.GetById(tableId)
                .Players
                .First(p => p.Id == playerId)
                .IsReady = true;
        }

        public void ChangeAutoTop(Guid tableId, Guid playerId, bool isAutoTop)
        {
            _allTables
                .GetById(tableId)
                .Players
                .First(p => p.Id == playerId)
                .IsAutoTop = isAutoTop;
        }

        // Statistics
        public async Task IncreaseNumberOfPlayedGamesAsync(Guid playerId, bool isWin)
        {
            await _unitOfWork.Players.IncreaseNumberOfPlayedGamesAsync(playerId, isWin);
        }
        
        public async Task IncreaseNumberOfSitNGoWinsAsync(Guid playerId)
        {
            await _unitOfWork.Players.IncreaseNumberOfSitNGoWinsAsync(playerId);
        }
        
        public async Task ChangeBestHandTypeAsync(Guid playerId, int newHandType)
        {
            await _unitOfWork.Players.ChangeBestHandTypeAsync(playerId, newHandType);
        }

        public async Task ChangeBiggestWinAsync(Guid playerId, int newBiggestWin)
        {
            await _unitOfWork.Players.ChangeBiggestWinAsync(playerId, newBiggestWin);
        }

        public async Task AddExperienceAsync(Guid playerId, int experience)
        {
            await _unitOfWork.Players.AddExperienceAsync(playerId, experience);
        }
    }
}