using ReminderBot.Entities;
using ReminderBot.Models.Models;
using Riok.Mapperly.Abstractions;

namespace ReminderBot.Mappings
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, UseReferenceHandling = true)]
    public static partial class TelegramUserModelMapper
    {
        //[MapperIgnoreSource(nameof(TelegramUserModel.Reminders))]
        //[MapperIgnoreTarget(nameof(TelegramUser.Reminders))]
        //[MapperIgnoreSource(nameof(TelegramUserModel.TelegramUserSettings))]
        //[MapperIgnoreTarget(nameof(TelegramUser.TelegramUserSettings))]
        public static partial TelegramUserModel MapTelegramUserModel(TelegramUser user);
        //[MapperIgnoreSource(nameof(TelegramUser.Reminders))]
        //[MapperIgnoreTarget(nameof(TelegramUserModel.Reminders))]
        //[MapperIgnoreSource(nameof(TelegramUser.TelegramUserSettings))]
        //[MapperIgnoreTarget(nameof(TelegramUserModel.TelegramUserSettings))]
        public static partial TelegramUser MapTelegramUser(TelegramUserModel user);
    }
}
