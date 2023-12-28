namespace ReminderBot.Models.Models
{
    public class TelegramUserSettingsModel
    {
        public int Id { get; set; }
        public int TelegramUserId { get; set; }
        public bool IsPaused { get; set; }
        public bool IsAutoDeleteMessages { get; set; }
        public int PostponeMinutes { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string? TimeZoneId { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public string DateTimeFormatType { get; set; } = null!;
        public string DateTimeFormat { get; set; } = null!;
    }
}
