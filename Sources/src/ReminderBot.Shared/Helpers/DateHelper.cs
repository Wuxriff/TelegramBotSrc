using System;
using NodaTime;

namespace ReminderBot.Shared.Helpers
{
    public static class DateHelper
    {
        public static DateTime ConvertUserToUtc(DateTime userTime, string userTimezoneId)
        {
            var zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimezoneId);

            return TimeZoneInfo.ConvertTimeToUtc(userTime, zoneInfo);
        }

        public static DateTime ConvertUserToLocal(DateTime userTime, string userTimezoneId)
        {
            var localZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var userZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimezoneId);
            var userUtcTime = TimeZoneInfo.ConvertTimeToUtc(userTime, userZoneInfo);

            return TimeZoneInfo.ConvertTime(userUtcTime, TimeZoneInfo.FindSystemTimeZoneById(localZone.Id));
        }
    }
}
