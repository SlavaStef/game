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
                .HasKey(p => p.Id);

            builder
                .Property(p => p.Gender)
                .IsRequired();

            builder
                .Property(p => p.Country)
                .IsRequired();

            builder
                .Property(p => p.RegistrationDate)
                .IsRequired();

            builder
                .Property(p => p.HandsSprite)
                .IsRequired();
            
            builder
                .Property(p => p.Experience)
                .IsRequired();
            
            builder
                .Property(p => p.TotalMoney)
                .IsRequired();
            
            builder
                .Property(p => p.MoneyBoxAmount)
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