using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresentController : BaseWebApiController
    {
        private readonly IPresentService _presentService;

        public PresentController(IPresentService presentService)
        {
            _presentService = presentService;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var presentsInfoDto = await Task.Run(() => _presentService.GetAllPresentsInfo());

            return Success(value: presentsInfoDto);
        }
    }
}