using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly UserManager<Player> _userManager;
        private readonly IMapper _mapper;

        public PlayerService(UserManager<Player> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<PlayerProfileDto> AddNewPlayer(string playerName)
        {
            var newPlayer = new Player
            {
                UserName = playerName,
                Country = null,
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

            var identityResult = await _userManager.CreateAsync(newPlayer);

            if (!identityResult.Succeeded)
                throw new Exception();

            return _mapper.Map<PlayerProfileDto>(newPlayer);
        }

        public async Task<PlayerProfileDto> Authenticate(Guid playerId)
        {
            var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
                throw new Exception();
            
            return _mapper.Map<PlayerProfileDto>(player);
        }
    }
}