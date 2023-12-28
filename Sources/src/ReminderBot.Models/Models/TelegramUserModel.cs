using System.Collections.Generic;

namespace ReminderBot.Models.Models
{
    public class TelegramUserModel
    {
        public TelegramUserModel()
        {
            Reminders = new List<ReminderModel>();
        }

        public int Id { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
        public bool IsActive { get; set; }

        public TelegramUserSettingsModel TelegramUserSettings { get; set; } = null!;
        public IList<ReminderModel> Reminders { get; set; }

        public bool IsLocationSet => !string.IsNullOrWhiteSpace(TelegramUserSettings.TimeZoneId);
    }
}
