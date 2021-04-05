using System;
using System.Threading.Tasks;
using PokerHand.Common;
using PokerHand.Common.Dto;
using PokerHand.Common.Helpers.Authorization;

namespace PokerHand.BusinessLogic.Interfaces
{
    public interface ILoginService
    {
        Task<PlayerProfileDto> AuthenticateWithPlayerId(Guid playerId);
        Task<ResultModel<PlayerProfileDto>> TryAuthenticateWithExternalProvider(string providerKey);
        Task CreateExternalLogin(Guid playerId, ExternalProviderName providerName, string providerKey);
        Task DeleteExternalLogin(Guid playerId);
    }
}