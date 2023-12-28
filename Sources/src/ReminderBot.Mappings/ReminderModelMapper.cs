using ReminderBot.Entities;
using ReminderBot.Models.Models;
using Riok.Mapperly.Abstractions;

namespace ReminderBot.Mappings
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, UseReferenceHandling = true)]
    public static partial class ReminderModelMapper
    {
        //[MapperIgnoreTarget(nameof(ReminderModel.Content))]
        //[MapperIgnoreTarget(nameof(ReminderModel.TelegramUser))]
        //[MapperIgnoreSource(nameof(Reminder.TelegramUser))]
        public static partial ReminderModel MapReminderModel(Reminder user);
        //[MapperIgnoreSource(nameof(ReminderModel.Content))]
        //[MapperIgnoreTarget(nameof(Reminder.TelegramUser))]
        //[MapperIgnoreSource(nameof(ReminderModel.TelegramUser))]
        public static partial Reminder MapReminder(ReminderModel user);
    }
}
