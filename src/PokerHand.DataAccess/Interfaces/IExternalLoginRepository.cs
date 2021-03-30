using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerHand.Common;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;

namespace PokerHand.DataAccess.Interfaces
{
    public interface IExternalLoginRepository
    {
        Task Add(Player player, ExternalProviderName externalProvider, string providerKey);
        Task<Guid> GetByProviderKey(string providerKey);
        Task RemoveByPlayerId(Guid playerId);
    }
}