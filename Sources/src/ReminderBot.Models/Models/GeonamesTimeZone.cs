namespace ReminderBot.Models.Models
{
    public class GeonamesTimeZone
    {
        public string? Sunrise { get; set; }
        public double Lng { get; set; }
        public string? CountryCode { get; set; }
        public int GmtOffset { get; set; }
        public int RawOffset { get; set; }
        public string? Sunset { get; set; }
        public string? TimezoneId { get; set; }
        public int DstOffset { get; set; }
        public string? CountryName { get; set; }
        public string? Time { get; set; }
        public double Lat { get; set; }
    }
}
