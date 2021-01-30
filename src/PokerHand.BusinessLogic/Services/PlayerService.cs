using System;
using System.Collections.Generic;
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
using PokerHand.Common.Helpers;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.BusinessLogic.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly List<Table> _allTables;
        private readonly UserManager<Player> _userManager;
        private readonly ILogger<TableService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public PlayerService(
            TablesCollection tablesCollection,
            UserManager<Player> userManager, 
            IMapper mapper, 
            ILogger<TableService> logger, 
            IUnitOfWork unitOfWork)
        {
            _allTables = tablesCollection.Tables;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            
        }

        public async Task<PlayerProfileDto> AddNewPlayer(string playerName)
        {
            var newPlayer = new Player
            {
                UserName = playerName,
                Country = "",
                RegistrationDate = DateTime.Now,
                Experience = 0,
                TotalMoney = 100000,
                CoinsAmount = 0,
                GamesPlayed = 0,
                BestHandType = HandType.None,
                GamesWon = 0,
                BiggestWin = 0,
                SitAndGoWins = 0
            };
            
            var addPlayerResult = await _userManager.CreateAsync(newPlayer);

            if (!addPlayerResult.Succeeded)
                return null;
            
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
        
        public async Task<bool> GetStackMoney(Guid playerId, int requiredAmount)
        {
            var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            if (player.TotalMoney < requiredAmount) 
                return false;

            await _unitOfWork.Players.SubtractTotalMoneyAsync(playerId, requiredAmount);
            
            return true;
        }

        public async Task ReturnToTotalMoney(Guid playerId, int amountToAdd)
        {
            await _unitOfWork.Players.AddTotalMoneyAsync(playerId, amountToAdd);
        }

        public async Task<bool> AddStackMoneyFromTotalMoney(Guid tableId, Guid playerId, int requiredAmount)
        {
            var player = await _userManager.Users.FirstAsync(p => p.Id == playerId);
            
            if (player.TotalMoney < requiredAmount) 
                return false;

            await _unitOfWork.Players.SubtractTotalMoneyAsync(playerId, requiredAmount);

            _allTables.First(t => t.Id == tableId)
                .Players.First(p => p.Id == playerId)
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
            _allTables
                .First(t => t.Id == tableId)
                .Players
                .First(p => p.Id == playerId).IsReady = true;
        }
    }
}