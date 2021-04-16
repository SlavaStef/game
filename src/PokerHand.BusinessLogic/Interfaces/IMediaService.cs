using System;
using System.Threading.Tasks;
using PokerHand.Common;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IMediaService
    {
        Task<ResultModel<string>> GetProfileImage(Guid playerId);
        Task<ResultModel<string>> UpdateProfileImage(Guid playerId, string newProfileImage);
        Task<ResultModel<string>> SetDefaultProfileImage(Guid playerId);
        Task<ResultModel<bool>> HasCustomProfileImage(Guid playerId);
    }
}