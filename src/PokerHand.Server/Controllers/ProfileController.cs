using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;
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
            
            Log.Information($"Update. updateResult: {JsonSerializer.Serialize(updateResult.Value)}");

            return updateResult.IsSuccess
                ? Success(value: updateResult.Value)
                : Error(message: updateResult.Message);
        }

        [HttpGet("getTotalMoney")]
        public async Task<IActionResult> GetTotalMoney(Guid playerId)
        {
            var totalMoney = await _playerService.GetTotalMoney(playerId);

            return Success(value: totalMoney);
        }
        
        [HttpPost("addTotalMoney")]
        public async Task<IActionResult> AddTotalMoney([FromBody]AddMoneyVM model)
        {
            await _playerService.AddTotalMoney(model.PlayerId, model.Amount);

            var newTotalMoney = await _playerService.GetTotalMoney(model.PlayerId);

            return Success(value: newTotalMoney);
        }
    }
}