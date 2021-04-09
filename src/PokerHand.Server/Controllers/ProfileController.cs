using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.ViewModels.Profile;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : Controller
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
            var profileDto = await _playerService.GetProfile(playerId);

            return profileDto is null 
                ? Problem() 
                : Ok(profileDto);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateProfileVM viewModel)
        {
            var profileDto =
                await _playerService.UpdateProfile(viewModel);

            return profileDto is null 
                ? Problem() 
                : Ok(profileDto);
        }
    }
}