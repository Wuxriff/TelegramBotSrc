using System;

namespace ReminderBot.Models.Models
{
    public class BotOptions
    {
        public string? Token { get; set; }
        public TimeSpan? CacheLifetime { get; set; }
        public string? GeonamesToken { get; set; }
        public TimeSpan? CheckDelay { get; set; }
        public int? ConfirmDelayMinutes { get; set; }
        public long? AdminTelegramUserId { get; set; }
    }
}
