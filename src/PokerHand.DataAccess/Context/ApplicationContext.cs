using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokerHand.Common.Entities;
using PokerHand.DataAccess.Configurations;

namespace PokerHand.DataAccess.Context
{
    public class ApplicationContext : IdentityDbContext<Player, IdentityRole<Guid>, Guid>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new PlayerConfiguration());
            
            base.OnModelCreating(builder);
        }
    }
}