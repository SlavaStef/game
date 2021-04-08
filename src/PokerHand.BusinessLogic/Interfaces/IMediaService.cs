using System;
using System.Threading.Tasks;
using PokerHand.Common;
using PokerHand.Common.Helpers.Media;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IMediaService
    {
        Task<ResultModel<string>> GetProfileImage(Guid playerId);
        Task<ResultModel<string>> UpdateProfileImage(Guid playerId, string newProfileImage);
        Task<ResultModel<string>> SetDefaultProfileImage(Guid playerId);
    }
}