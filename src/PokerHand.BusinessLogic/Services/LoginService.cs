using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly UserManager<Player> _userManager;
        private readonly IMapper _mapper;

        public LoginService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<Player> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        
        public async Task<ResultModel<PlayerProfileDto>> AuthenticateWithPlayerId(Guid playerId)
        {
            var result = new ResultModel<PlayerProfileDto>();
            var player = await _unitOfWork.Players.GetPlayerAsync(playerId);

            return player is not null
                ? new ResultModel<PlayerProfileDto> {IsSuccess = true, Value = _mapper.Map<PlayerProfileDto>(player)}
                : new ResultModel<PlayerProfileDto> {IsSuccess = false, Message = "Player not found"};
        }

        public async Task<ResultModel<PlayerProfileDto>> TryAuthenticateWithExternalProvider(string providerKey)
        {
            try
            {
                Log.Information($"TryAuthenticateWithExternalProvider. providerKey: {providerKey}");
                var playerId = await _unitOfWork.ExternalLogins.GetByProviderKey(providerKey);

                if (playerId == Guid.Empty)
                {
                    Log.Information("TryAuthenticateWithExternalProvider. playerId is empty");
                    return new ResultModel<PlayerProfileDto> { IsSuccess = false };
                }

                Log.Information("TryAuthenticateWithExternalProvider. Try to get player by Id");
                var player = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == playerId);

                if (player is null)
                {
                    Log.Information("TryAuthenticateWithExternalProvider. Player not found");
                    return new ResultModel<PlayerProfileDto> {IsSuccess = false, Message = "Player not found"};
                }
                
                return new ResultModel<PlayerProfileDto>
                {
                    IsSuccess = true,
                    Value = _mapper.Map<PlayerProfileDto>(player)
                };
            }
            catch (Exception e)
            {
                Log.Information(e.Message);
                Log.Information(e.StackTrace);
                throw;
            }
            
        }

        public async Task<ResultModel> CreateExternalLogin(Guid playerId, ExternalProviderName providerName, string providerKey)
        {
            var result = new ResultModel();
            var player = await _unitOfWork.Players.GetPlayerAsync(playerId);

            if (player is null)
            {
                result.Message = $"Player {playerId} doesn't exist";
                result.IsSuccess = false;
                return result;
            }

            await _unitOfWork.ExternalLogins.Add(player, providerName, providerKey);

            result.IsSuccess = true;
            return result;
        }

        public async Task<ResultModel<bool>> DeleteExternalLogin(Guid playerId)
        {
            await _unitOfWork.ExternalLogins.RemoveByPlayerId(playerId);

            return new ResultModel<bool> {IsSuccess = true, Value = true};
        }
    }
}