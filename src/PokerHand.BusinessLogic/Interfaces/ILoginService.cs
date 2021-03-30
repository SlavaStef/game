using System;
using System.Threading.Tasks;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface ILoginService
    {
        Task<ResultModel<PlayerProfileDto>> TryAuthenticate(string providerKey);
        Task CreateExternalLogin(Guid playerId, ExternalProviderName providerName, string providerKey);
        Task DeleteExternalLogin(Guid playerId);
    }
}