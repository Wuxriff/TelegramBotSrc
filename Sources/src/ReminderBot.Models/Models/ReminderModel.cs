using System;

namespace ReminderBot.Models.Models
{
    public class ReminderModel
    {
        public int Id { get; set; }
        public int TelegramUserId { get; set; }
        public string Base64Content { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedDateUtc { get; set; }
        public DateTime ReminderDateUtc { get; set; }
        public DateTime OriginalReminderDateUtc { get; set; }
        public DateTime ReminderDate { get; set; }
        public DateTime OriginalReminderDate { get; set; }
        public DateTime? DateSentUtc { get; set; }
        public DateTime? OriginalDateSentUtc { get; set; }
        public bool IsConfirmed { get; set; }
        public int? MessageId { get; set; }

        public TelegramUserModel TelegramUser { get; set; } = null!;
    }
}
