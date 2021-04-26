using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.Common.ViewModels.Auth;
using PokerHand.Common.ViewModels.Profile;
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
        private readonly IPlayersOnline _allPlayers;

        public AuthController(
            ILoginService loginService, 
            IPlayerService playerService, 
            IMediaService mediaService, 
            IPlayersOnline allPlayers)
        {
            _loginService = loginService;
            _playerService = playerService;
            _mediaService = mediaService;
            _allPlayers = allPlayers;
        }

        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateVM viewModel)
        {
            var authenticateResult = await _loginService.AuthenticateWithPlayerId(viewModel.UserId);

            if (authenticateResult.IsSuccess is false)
                return Error(message: authenticateResult.Message);
            
            _allPlayers.AddOrUpdate(viewModel.UserId, viewModel.ConnectionId);
            
            return Success(value: authenticateResult.Value);
        }

        [HttpGet]
        [Route("tryAuthenticateWithExternalProvider")]
        public async Task<IActionResult> TryAuthenticateWithExternalProvider(string providerKey)
        {
            var authenticateResult = await _loginService.TryAuthenticateWithExternalProvider(providerKey);

            return authenticateResult.IsSuccess
                ? Success(value: authenticateResult.Value)
                : Error(message: authenticateResult.Message);
        }
        
        [HttpPost]
        [Route("registerAsGuest")]
        public async Task<IActionResult> RegisterAsGuest([FromBody] RegisterAsGuestVM viewModel)
        {
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var createPlayerResult = await _playerService.CreatePlayer(viewModel.UserName, viewModel.Gender,
                viewModel.HandsSprite, ipAddress);
            
            return createPlayerResult.IsSuccess
                ? Success(value: createPlayerResult.Value)
                : Error(message: createPlayerResult.Message);
        }
        
        [HttpPost]
        [Route("registerWithExternalProvider")]
        public async Task<IActionResult> RegisterWithExternalProvider([FromBody] RegisterWithExternalProviderVM viewModel)
        {
            Log.Information($"NEW IMAGE: {viewModel}");
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var createPlayerResult = await _playerService.CreatePlayer(viewModel.UserName, viewModel.Gender,
                viewModel.HandsSprite, ipAddress);

            if (createPlayerResult.IsSuccess is false)
                return Error(message: createPlayerResult.Message);

            var createExternalLoginResult = await _loginService.CreateExternalLogin(createPlayerResult.Value.Id,
                viewModel.ProviderName, viewModel.ProviderKey);

            return createExternalLoginResult.IsSuccess
                ? Success(value: createPlayerResult.Value)
                : Error(message: createPlayerResult.Message);
        }
            
        [HttpPost]
        [Route("addProvider")]
        public async Task<IActionResult> AddExternalProvider([FromBody] AddExternalProviderVM viewModel)
        {
            Log.Information($"ADD IMAGE: {viewModel.ProfileImage}");
            await _loginService.CreateExternalLogin(viewModel.PlayerId, viewModel.ProviderName, viewModel.ProviderKey);

            var isCustomProfileImage = await _mediaService.HasCustomProfileImage(viewModel.PlayerId);
            if (isCustomProfileImage.IsSuccess is true && isCustomProfileImage.Value is false)
                await _mediaService.UpdateProfileImage(viewModel.PlayerId, viewModel.ProfileImage);

            var getProfileResult = await _playerService.GetProfile(viewModel.PlayerId);

            return getProfileResult.IsSuccess
                ? Success(value: getProfileResult.Value)
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