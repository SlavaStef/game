using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PokerHand.Common.Helpers;

namespace PokerHand.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseWebApiController : ControllerBase
    {
        private readonly JsonSerializerSettings _jsonSettings;

        protected BaseWebApiController()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

        protected IActionResult Success(string message = null, object value = null, int statusCode = 200)
        {
            var result = new ResponseContent { Message = message, Value = value };

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(result, _jsonSettings),
                ContentType = "application/json",
                StatusCode = statusCode
            };
        }

        protected IActionResult Error(string message = null, string title = "Error", int statusCode = 400)
        {
            var result = new ResponseContent() { Message = message };

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(result, _jsonSettings),
                ContentType = "application/json",
                StatusCode = statusCode
            };
        }
    }
}