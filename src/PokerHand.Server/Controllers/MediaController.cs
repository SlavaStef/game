using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common.Helpers.Media;
using PokerHand.Common.ViewModels.Media;

namespace PokerHand.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : Controller
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }
        
        [HttpGet]
        [Route("getProfileImage")]
        public async Task<IActionResult> GetProfileImage(Guid playerId)
        {
            var getResult = await _mediaService.GetProfileImage(playerId);

            if (getResult.IsSuccess is false)
                return Problem(getResult.Message);

            var avatar = new Avatar {BinaryImage = getResult.Value, PlayerId = playerId};
            return Ok(avatar);
        }

        [HttpPost]
        [Route("updateProfileImage")]
        public async Task<IActionResult> UpdateProfileImage([FromBody] UpdateProfileImageVM viewModel)
        {
            var updateResult =
                await _mediaService.UpdateProfileImage(viewModel.PlayerId, viewModel.NewProfileImage);

            if (updateResult.IsSuccess is false)
                return Problem(updateResult.Message);

            var avatar = new Avatar {BinaryImage = updateResult.Value, PlayerId = viewModel.PlayerId};

            return Ok(avatar);
        }

        [HttpGet]
        [Route("removeProfileImage")]
        public async Task<IActionResult> RemoveProfileImage(Guid playerId)
        {
            var removeResult = await _mediaService.SetDefaultProfileImage(playerId);
            
            if (removeResult.IsSuccess is false)
                return Problem(removeResult.Message);
            
            var avatar = new Avatar {BinaryImage = removeResult.Value, PlayerId = playerId};

            return Ok(avatar);
        }
    }
}