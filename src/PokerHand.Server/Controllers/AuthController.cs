using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.BusinessLogic.Services;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.Server.Hubs;
using PokerHand.Server.Hubs.Interfaces;
using Serilog;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseWebApiController
    {
        private readonly ILoginService _loginService;
        private readonly IPlayerService _playerService;
        private readonly IMediaService _mediaService;

        public AuthController(ILoginService loginService, IPlayerService playerService, IMediaService mediaService)
        {
            _loginService = loginService;
            _playerService = playerService;
            _mediaService = mediaService;
        }

        [HttpPost]
        [Route("addProvider")]
        public async Task<IActionResult> AddExternalProvider(Guid playerId, ExternalProviderName providerName,
            string providerKey, string profileImage)
        {
            await _loginService.CreateExternalLogin(playerId, providerName, providerKey);

            var isCustomProfileImage = await _mediaService.HasCustomProfileImage(playerId);
            if (isCustomProfileImage.IsSuccess is true && isCustomProfileImage.Value is false)
                await _mediaService.UpdateProfileImage(playerId, profileImage);

            var getProfileResult = await _playerService.GetProfile(playerId);

            return getProfileResult.IsSuccess
                ? Success(value: getProfileResult)
                : Error(message: getProfileResult.Message);
        }

        [HttpDelete]
        [Route("deleteProvider")]
        public async Task<IActionResult> DeleteExternalProvider(Guid playerId)
        {
            var deleteResult = await _loginService.DeleteExternalLogin(playerId);

            return deleteResult.IsSuccess
                ? Success(value: deleteResult.Value)
                : Error(message: deleteResult.Message);
        }
    }
}