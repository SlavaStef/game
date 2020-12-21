using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokerHand.Common.Entities;

namespace PokerHand.DataAccess.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder
                .Property(p => p.Country)
                .HasMaxLength(20)
                .IsRequired();

            builder
                .Property(p => p.RegistrationDate)
                .IsRequired();

            builder
                .Property(p => p.Experience)
                .IsRequired();
            
            builder
                .Property(p => p.TotalMoney)
                .IsRequired();
            
            builder
                .Property(p => p.CoinsAmount)
                .IsRequired();
            
            builder
                .Property(p => p.GamesPlayed)
                .IsRequired();
            
            builder
                .Property(p => p.BestHandType)
                .IsRequired();
            
            builder
                .Property(p => p.GamesWon)
                .IsRequired();
            
            builder
                .Property(p => p.BiggestWin)
                .IsRequired();
            
            builder
                .Property(p => p.SitAndGoWins)
                .IsRequired();
        }
    }
}