using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReminderBot.Entities;

namespace ReminderBot.DataAccess.Configuration
{
    public class TelegramUserSettingsConfiguration : IEntityTypeConfiguration<TelegramUserSettings>
    {
        public void Configure(EntityTypeBuilder<TelegramUserSettings> builder)
        {
            builder.ToTable("TelegramUserSettings");
            builder.HasKey(x => x.Id);
        }
    }
}
