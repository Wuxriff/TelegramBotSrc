using ReminderBot.Entities;
using ReminderBot.Models.Models;
using Riok.Mapperly.Abstractions;

namespace ReminderBot.Mappings
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, UseReferenceHandling = true)]
    public static partial class TelegramUserSettingsModelMapper
    {
        //[MapperIgnoreSource(nameof(TelegramUserSettings.TelegramUser))]
        public static partial TelegramUserSettingsModel MapTelegramUserSettingsModel(TelegramUserSettings user);
        //[MapperIgnoreTarget(nameof(TelegramUserSettings.TelegramUser))]
        public static partial TelegramUserSettings MapTelegramUserSettings(TelegramUserSettingsModel user);
    }
}
