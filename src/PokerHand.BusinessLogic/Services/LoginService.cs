using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.DataAccess.Interfaces;
using Serilog;

namespace PokerHand.BusinessLogic.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LoginService> _logger;
        
        public LoginService(
            IUnitOfWork unitOfWork,
            IMapper mapper, 
            ILogger<LoginService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task<PlayerProfileDto> AuthenticateWithPlayerId(Guid playerId)
        {
            var player = await _unitOfWork.Players.GetPlayerAsync(playerId);

            return player is null 
                ? null
                : _mapper.Map<PlayerProfileDto>(player);
        }

        public async Task<ResultModel<PlayerProfileDto>> TryAuthenticateWithExternalProvider(string providerKey)
        {
            var playerId = await _unitOfWork.ExternalLogins.GetByProviderKey(providerKey);
                
            if (playerId == Guid.Empty)
                return new ResultModel<PlayerProfileDto> { IsSuccess = false };

            var player = await _unitOfWork.Players.GetPlayerAsync(playerId);

            if (player is null)
                return new ResultModel<PlayerProfileDto> {IsSuccess = false, Message = "Player not found"};

            return new ResultModel<PlayerProfileDto>
            {
                IsSuccess = true,
                Value = _mapper.Map<PlayerProfileDto>(player)
            };
        }

        public async Task CreateExternalLogin(Guid playerId, ExternalProviderName providerName, string providerKey)
        {
            var player = await _unitOfWork.Players.GetPlayerAsync(playerId);

            if (player is null)
            {
                _logger.LogInformation($"Player {playerId} doesn't exist");
                return;
            }

            await _unitOfWork.ExternalLogins.Add(player, providerName, providerKey);
        }

        public async Task DeleteExternalLogin(Guid playerId)
        {
            await _unitOfWork.ExternalLogins.RemoveByPlayerId(playerId);
        }
    }
}