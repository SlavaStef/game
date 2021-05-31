using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokerHand.Common.Entities.Chat;

namespace PokerHand.DataAccess.Configurations
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder
                .HasOne(c => c.FirstPlayer)
                .WithMany(p => p.ConversationsFirst)
                .HasForeignKey(c => c.FirstPlayerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            
            builder
                .HasOne(c => c.SecondPlayer)
                .WithMany(p => p.ConversationsSecond)
                .HasForeignKey(c => c.SecondPlayerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}