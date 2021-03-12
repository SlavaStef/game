using System;
using System.Threading.Tasks;
using PokerHand.Common.Helpers;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface IMediaService
    {
        Task<ResultModel<byte[]>> GetProfileImage(string playerIdJson);
        Task<ResultModel<byte[]>> UpdateProfileImage(string imageJson);
        Task<ResultModel<byte[]>> SetDefaultProfileImage(string playerIdJson);
    }
}