using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PokerHand.Common.Helpers;

namespace PokerHand.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseWebApiController : ControllerBase
    {
        protected IActionResult Success(string message = null, object value = null, int statusCode = 200)
        {
            var result = new ResponseContent { Message = message, Value = value };

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json",
                StatusCode = statusCode
            };
        }

        protected IActionResult Error(string message = null, string title = "Error", int statusCode = 400)
        {
            var result = new ResponseContent { Message = message };

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json",
                StatusCode = statusCode
            };
        }
    }
}