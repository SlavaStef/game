using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Helpers.Media;
using PokerHand.Common.ViewModels.Media;
using PokerHand.Common.ViewModels.Profile;
using Serilog;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : BaseWebApiController
    {
        private readonly IPlayerService _playerService;

        public ProfileController(IPlayerService playerService)
        {
            _playerService = playerService;
        }
        
        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get(Guid playerId)
        {
            var getProfileResult = await _playerService.GetProfile(playerId);

            return getProfileResult.IsSuccess
                ? Success(value: getProfileResult.Value)
                : Error(message: getProfileResult.Message);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateProfileVM viewModel)
        {
            var updateResult = await _playerService.UpdateProfile(viewModel);

            return updateResult.IsSuccess
                ? Success(value: updateResult.Value)
                : Error(message: updateResult.Message);
        }
    }
}