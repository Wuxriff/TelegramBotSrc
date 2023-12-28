using System;
using Microsoft.Extensions.Options;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Models;

namespace ReminderBot.BL
{
    public class BotConfigurationService : IBotConfigurationService
    {
        private readonly BotOptions _options;

        private static readonly TimeSpan _defaultCacheTimespan = new(0, 30, 0);
        private static readonly TimeSpan _defaultDelay = new(0, 0, 30);
        private const int _defaultConfirmDelayMinutes = 2;

        public BotConfigurationService(IOptions<BotOptions> options)
        {
            _options = options.Value;
        }

        public string GetApiToken()
        {
            if (string.IsNullOrWhiteSpace(_options.Token))
            {
                throw new NullReferenceException("Empty telegram bot token value");
            }

            return _options.Token;
        }

        public TimeSpan GetCacheLifetime()
        {
            return _options.CacheLifetime ?? _defaultCacheTimespan;
        }

        public string GetGeonamesToken()
        {
            if (string.IsNullOrWhiteSpace(_options.GeonamesToken))
            {
                throw new NullReferenceException("Empty Geonames token value");
            }

            return _options.GeonamesToken;
        }

        public TimeSpan GetCheckDelay()
        {
            return _options.CheckDelay ?? _defaultDelay;
        }

        public int GetConfirmDelayMinutes()
        {
            return _options.ConfirmDelayMinutes ?? _defaultConfirmDelayMinutes;
        }

        public long GetAdminTelegramUserId()
        {
            return _options.AdminTelegramUserId ?? -1;
        }
    }
}
