using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokerHand.Common.Entities;

namespace PokerHand.DataAccess.Configurations
{
    public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
    {
        public void Configure(EntityTypeBuilder<ExternalLogin> builder)
        {
            builder
                .HasKey(p => new { p.ProviderKey, LoginProvider = p.ProviderName });

            builder
                .HasIndex(p => p.ProviderKey);
            
            builder
                .Property(p => p.ProviderName)
                .IsRequired();

            builder
                .Property(p => p.ProviderKey)
                .IsRequired();

            builder
                .Property(p => p.PlayerId)
                .IsRequired();
        }
    }
}