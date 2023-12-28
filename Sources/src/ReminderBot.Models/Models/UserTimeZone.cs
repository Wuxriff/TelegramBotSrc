using System;

namespace ReminderBot.Models.Models
{
    public class UserTimeZone
    {
        public string TimeZoneId { get; set; } = null!;
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; } = null!;
    }
}
