using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokerHand.Common.Entities.Chat;

namespace PokerHand.DataAccess.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder
                .Property(m => m.Text)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}