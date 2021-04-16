using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.Player;
using PokerHand.DataAccess.Configurations;

namespace PokerHand.DataAccess.Context
{
    public class ApplicationContext : IdentityDbContext<Player, IdentityRole<Guid>, Guid>
    {
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<ExternalLogin> ExternalLogins { get; set; }

        static ApplicationContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ExternalProviderName>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Gender>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<CountryCode>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<HandsSpriteType>();
            NpgsqlConnection.GlobalTypeMapper.MapEnum<HandType>();
        }

        //public ApplicationContext() { }
        
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresEnum<ExternalProviderName>();
            builder.HasPostgresEnum<Gender>();
            builder.HasPostgresEnum<CountryCode>();
            builder.HasPostgresEnum<HandsSpriteType>();
            builder.HasPostgresEnum<HandType>();

            builder.ApplyConfiguration(new PlayerConfiguration());
            builder.ApplyConfiguration(new ExternalLoginConfiguration());

            base.OnModelCreating(builder);
        }
        
        public override ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}