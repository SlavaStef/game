using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : BaseWebApiController
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        [HttpGet]
        [Route("getInfo")]
        public async Task<IActionResult> GetInfo(string tableTitle)
        {
            var getResult = _tableService.GetTableInfo(tableTitle);

            return getResult.IsSuccess
                ? Success(value: getResult.Value)
                : Error(message: getResult.Message);
        }

        [HttpGet]
        [Route("getAllInfo")]
        public async Task<IActionResult> GetAllInfo()
        {
            var allTablesInfo = _tableService.GetAllTablesInfo();

            return Success(value: allTablesInfo);
        }
    }
}