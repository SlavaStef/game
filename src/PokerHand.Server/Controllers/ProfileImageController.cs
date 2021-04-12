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
    public class ProfileImageController : BaseWebApiController
    {
        private readonly IMediaService _mediaService;

        public ProfileImageController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }
        
        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get(Guid playerId)
        {
            var getResult = await _mediaService.GetProfileImage(playerId);

            return getResult.IsSuccess
                ? Success(value: new Avatar {BinaryImage = getResult.Value, PlayerId = playerId})
                : Error(message: getResult.Message);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateProfileImageVM viewModel)
        {
            var updateResult =
                await _mediaService.UpdateProfileImage(viewModel.PlayerId, viewModel.NewProfileImage);

            return updateResult.IsSuccess
                ? Success(value: new Avatar {BinaryImage = updateResult.Value, PlayerId = viewModel.PlayerId})
                : Error(updateResult.Message);
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete(Guid playerId)
        {
            var removeResult = await _mediaService.SetDefaultProfileImage(playerId);

            return removeResult.IsSuccess
                ? Success(value: new Avatar {BinaryImage = removeResult.Value, PlayerId = playerId})
                : Error(removeResult.Message);
        }
    }
}