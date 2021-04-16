using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoneyBoxController : BaseWebApiController
    {
        private readonly IMoneyBoxService _moneyBoxService;

        public MoneyBoxController(IMoneyBoxService moneyBoxService)
        {
            _moneyBoxService = moneyBoxService;
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get(Guid playerId)
        {
            var getResult = await _moneyBoxService.GetMoneyBoxAmount(playerId);

            return getResult.IsSuccess
                ? Success(value: getResult.Value)
                : Error(message: getResult.Message);
        }
        
        [HttpGet]
        [Route("open")]
        public async Task<IActionResult> Open(Guid playerId)
        {
            var openResult = await _moneyBoxService.OpenMoneyBox(playerId);

            return openResult.IsSuccess
                ? Success(value: openResult.Value)
                : Error(message: openResult.Message);
        }
    }
}