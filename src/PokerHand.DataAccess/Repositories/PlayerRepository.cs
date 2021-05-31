﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Interfaces;

namespace PokerHand.DataAccess.Repositories
{
    public class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        public PlayerRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<Player> GetPlayerAsync(Guid playerId)
        {
            return await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
        }
        
        public async Task<bool> PlayerExistsAsync(Guid playerId)
        {
            return await _context.Players.AnyAsync(p => p.Id == playerId);
        }

        public async Task AddTotalMoneyAsync(Guid playerId, int amount)
        {
            var player = await _context.Players.FirstAsync(p => p.Id == playerId);
            
            player.TotalMoney += amount;
            
            await _context.SaveChangesAsync();
        }
        
        public async Task SubtractTotalMoneyAsync(Guid playerId, int amount)
        {
            var player = await _context.Players.FirstAsync(p => p.Id == playerId);

            player.TotalMoney -= amount;
            
            await _context.SaveChangesAsync();
        }

        public async Task AddCoinsAsync(Guid playerId, int amount)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            player.CoinsAmount += amount;
 
            await _context.SaveChangesAsync();
        }

        public async Task SubtractCoinsAsync(Guid playerId, int amount)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            
            player.CoinsAmount -= amount;
 
            await _context.SaveChangesAsync();
        }

        // MoneyBox
        public async Task<int> GetMoneyBoxAmountAsync(Guid playerId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            return player.MoneyBoxAmount;
        }

        public async Task<int> IncreaseMoneyBoxAmountAsync(Guid playerId, int amount)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            player.MoneyBoxAmount += amount;
                
            await _context.SaveChangesAsync();

            return player.MoneyBoxAmount;
        }

        public async Task<int> OpenMoneyBoxAsync(Guid playerId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            player.TotalMoney += player.MoneyBoxAmount;
            player.MoneyBoxAmount = 0;

            await _context.SaveChangesAsync();

            return player.TotalMoney;
        }

        // Statistics
        public async Task IncreaseNumberOfPlayedGamesAsync(Guid playerId, bool isWin)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            player.GamesPlayed++;

            if (isWin)
                player.GamesWon++;

            await _context.SaveChangesAsync();
        }

        public async Task IncreaseNumberOfSitNGoWinsAsync(Guid playerId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            player.SitAndGoWins++;

            await _context.SaveChangesAsync();
        }

        public async Task ChangeBestHandTypeAsync(Guid playerId, int newHandType)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            if ((int) player.BestHandType < newHandType)
                player.BestHandType = (HandType)newHandType;
            
            await _context.SaveChangesAsync();
        }

        public async Task ChangeBiggestWinAsync(Guid playerId, int newBiggestWin)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            if (player.BiggestWin < newBiggestWin)
                player.BiggestWin = newBiggestWin;
            
            await _context.SaveChangesAsync();
        }

        public async Task AddExperienceAsync(Guid playerId, int numberOfExperience)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);

            player.Experience += numberOfExperience;
            
            await _context.SaveChangesAsync();
        }
    }
}