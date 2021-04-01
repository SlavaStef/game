﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokerHand.Common;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;
using Serilog;

namespace PokerHand.DataAccess.Repositories
{
    public class ExternalLoginRepository : Repository<ExternalLogin>, IExternalLoginRepository
    {
        private readonly ILogger<UnitOfWork> _logger;
        public ExternalLoginRepository(ApplicationContext context, ILogger<UnitOfWork> logger) : base(context)
        {
            _logger = logger;
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
                _logger.LogInformation($"ExternalLoginRepository.GetByProviderKey. ProviderKey: {providerKey}");
                var login = await _context.ExternalLogins
                    .FirstOrDefaultAsync(l => l.ProviderKey == providerKey);

                if (login is null)
                {
                    _logger.LogInformation("ExternalLoginRepository. Login not found");
                    return Guid.Empty;
                }
            
                _logger.LogInformation($"ExternalLoginRepository. {JsonSerializer.Serialize(login)}");
                return login.PlayerId;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
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