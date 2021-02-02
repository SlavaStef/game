using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokerHand.Common.Entities;
using PokerHand.Common.Entities.Chat;
using PokerHand.DataAccess.Configurations;

namespace PokerHand.DataAccess.Context
{
    public class ApplicationContext : IdentityDbContext<Player, IdentityRole<Guid>, Guid>
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new PlayerConfiguration());
            builder.ApplyConfiguration(new ConversationConfiguration());
            builder.ApplyConfiguration(new MessageConfiguration());
            
            base.OnModelCreating(builder);
        }
        
        public override ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}