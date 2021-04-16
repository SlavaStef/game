using System;
using System.Threading.Tasks;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IExternalLoginRepository : IRepository<ExternalLogin>
    {
        Task Add(Player player, ExternalProviderName externalProvider, string providerKey);
        Task<Guid> GetByProviderKey(string providerKey);
        Task RemoveByPlayerId(Guid playerId);
    }
}