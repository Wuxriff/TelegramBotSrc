using System.Collections.Generic;

namespace ReminderBot.Entities
{
    public class TelegramUser
    {
        public TelegramUser()
        {
            Reminders = new List<Reminder>();
        }

        public int Id { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
        public bool IsActive { get; set; }

        public virtual TelegramUserSettings TelegramUserSettings { get; set; } = null!;
        public virtual IList<Reminder> Reminders { get; set; }
    }
}
