using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ReminderBot.Shared.Extensions;

namespace ReminderBot.Shared
{
    public static class SystemConstants
    {
        //Database
        public const string DatabaseName = "Filename=Data/Bot.db";

        //Telegram
        public const string TelegramToken = nameof(TelegramToken);
        public const string TelegramBotUserName = nameof(TelegramBotUserName);
        public const string TelegramBotUser = nameof(TelegramBotUser);
        public const string TelegramBotUserHandler = nameof(TelegramBotUserHandler);

        //Localization & settings
        public const string EnLoc = "en";
        public const string RuLoc = "ru";
        public const string BeLoc = "be";
        public const string UkLoc = "uk";

        public static Dictionary<string, string> AvailableLanguages = new Dictionary<string, string>
        {
            { EnLoc, "US".IsoCountryCodeToFlagEmoji() },
            { RuLoc, "RU".IsoCountryCodeToFlagEmoji() },
            { BeLoc, "BY".IsoCountryCodeToFlagEmoji() },
            { UkLoc, "UA".IsoCountryCodeToFlagEmoji() },
        };

        public const string UsTypeFormat = nameof(UsTypeFormat);
        public const string EuTypeFormat = nameof(EuTypeFormat);
        public const string UniTypeFormat = nameof(UniTypeFormat);
        public const string LocationTypeFormat = nameof(LocationTypeFormat);

        private static readonly CultureInfo _usCultureInfo = new CultureInfo("en-US");
        private static readonly CultureInfo _eUCultureInfo = new CultureInfo("en-GB");

        public static Dictionary<string, (CultureInfo Culture, string Format)> DateTimeFormats = new Dictionary<string, (CultureInfo Culture, string Format)>()
        {
            { UsTypeFormat, (_usCultureInfo, _usCultureInfo.GetDateTimeFormat()) },
            { EuTypeFormat, (_eUCultureInfo, _eUCultureInfo.GetDateTimeFormat()) },
            { UniTypeFormat, (CultureInfo.InvariantCulture, "yyyy.MM.dd HH:mm:ss") }
        };

        public static Dictionary<string, string> DateTimeFormatNames = new Dictionary<string, string>()
        {
            { UsTypeFormat, "US" },
            { EuTypeFormat, "EU" },
            { UniTypeFormat, "Universal" },
            { LocationTypeFormat, "Location" },
        };
    }
}
