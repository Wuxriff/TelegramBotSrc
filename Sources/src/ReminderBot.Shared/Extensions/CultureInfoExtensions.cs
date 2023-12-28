using System.Globalization;

namespace ReminderBot.Shared.Extensions
{
    public static class CultureInfoExtensions
    {
        public static string GetDateTimeFormat(this CultureInfo cultureInfo)
        {
            var format = cultureInfo.DateTimeFormat;
            var dateFormat = format.ShortDatePattern;
            var timeFormat = format.LongTimePattern;

            return $"{dateFormat} {timeFormat}";
        }
    }
}
