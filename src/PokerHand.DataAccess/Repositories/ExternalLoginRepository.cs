using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;
using Serilog;

namespace PokerHand.DataAccess.Repositories
{
    public class ExternalLoginRepository : Repository<ExternalLogin>, IExternalLoginRepository
    {
        public ExternalLoginRepository(ApplicationContext context) : base(context)
        {
        }
        
        public async Task Add(Player player, ExternalProviderName externalProvider, string providerKey)
        {
            await _context.ExternalLogins
                .AddAsync(new ExternalLogin
                {
                    ProviderName = externalProvider,
                    Player = player,
                    PlayerId = player.Id,
                    ProviderKey = providerKey
                });

            await _context.SaveChangesAsync();
        }
        
        public async Task<Guid> GetByProviderKey(string providerKey)
        {
            try
            {
                Log.Information($"ExternalLoginRepository.GetByProviderKey. ProviderKey: {providerKey}");
                var login = await _context.ExternalLogins
                    .FirstOrDefaultAsync(l => l.ProviderKey == providerKey);

                if (login is null)
                {
                    Log.Information("ExternalLoginRepository. Login not found");
                    return Guid.Empty;
                }
            
                Log.Information($"ExternalLoginRepository. {JsonSerializer.Serialize(login)}");
                return login.PlayerId;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                throw;
            }
        }
        
        public async Task RemoveByPlayerId(Guid playerId)
        {
            var login = await _context.ExternalLogins
                .FirstOrDefaultAsync(l => l.PlayerId == playerId);

            if (login is null)
                return;
            
            _context.ExternalLogins.Remove(login);

            await _context.SaveChangesAsync();
        }
    }
}