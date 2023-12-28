using System;

namespace ReminderBot.BL.Interfaces
{
    public interface IBotConfigurationService
    {
        string GetApiToken();
        TimeSpan GetCacheLifetime();
        string GetGeonamesToken();
        TimeSpan GetCheckDelay();
        int GetConfirmDelayMinutes();
        long GetAdminTelegramUserId();
    }
}
