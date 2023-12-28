using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReminderBot.Entities;

namespace ReminderBot.DataAccess.Configuration
{
    public class TelegramUserConfiguration : IEntityTypeConfiguration<TelegramUser>
    {
        public void Configure(EntityTypeBuilder<TelegramUser> builder)
        {
            builder.ToTable("TelegramUsers");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.TelegramUserSettings).WithOne(x => x.TelegramUser)
                .HasForeignKey<TelegramUserSettings>(x => x.TelegramUserId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Reminders).WithOne(x => x.TelegramUser).HasForeignKey(x => x.TelegramUserId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
