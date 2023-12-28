using System;

namespace ReminderBot.Entities
{
    public class Reminder
    {
        public int Id { get; set; }
        public int TelegramUserId { get; set; }
        public string Base64Content { get; set; } = null!;
        public DateTime CreatedDateUtc { get; set; }
        public DateTime ReminderDateUtc { get; set; } // при обновлении tzdata надо будет всего лишь пересчитать эти ReminderDateUtc
        public DateTime OriginalReminderDateUtc { get; set; } // при обновлении tzdata надо будет всего лишь пересчитать эти OriginalReminderDateUtc
        public DateTime ReminderDate { get; set; }
        public DateTime OriginalReminderDate { get; set; }
        public DateTime? DateSentUtc { get; set; }
        public DateTime? OriginalDateSentUtc { get; set; }
        public bool IsConfirmed { get; set; }
        public int? MessageId { get; set; }

        public virtual TelegramUser TelegramUser { get; set; } = null!;
    }
}
