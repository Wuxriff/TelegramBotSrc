using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Enums;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers.System
{
    public class SettingsHandler : BaseMessageHandler
    {
        private const int _minPostpone = 1;
        private const int _maxPostpone = 120;
        private static readonly DateTime _exampleDatetime = new DateTime(1970, 12, 31, 18, 42, 30);

        private int? _messageId = null;

        public SettingsHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandSettings;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = await GetUser();

            if (State == BotHandlerStates.Starting)
            {
                var replyMarkup = GetSettingsMarkup(user);

                _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Settings_MainHeaderMessage", user.LanguageCode), replyMarkup, _messageId);

                State = BotHandlerStates.Waiting_Input_Callback;
                OperationState = BotHandlerOperationStates.Settings_Waiting;
                return (this, false);
            }

            if (OperationState == BotHandlerOperationStates.Settings_Waiting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_Language)
                {
                    var markup = GetLanguageMarkup();

                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Settings_LanguageHeaderMessage", user.LanguageCode), markup);

                    State = BotHandlerStates.Waiting_Input_Callback;
                    OperationState = BotHandlerOperationStates.Settings_Language_Waiting;
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_Location)
                {
                    var markup = GetShareLocationMarkup(user.LanguageCode);

                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Settings_LocationHeaderMessage", user.LanguageCode), markup);

                    State = BotHandlerStates.Waiting_Input_Message;
                    OperationState = BotHandlerOperationStates.Settings_Location_Waiting;
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_AutoDelete)
                {
                    var replyMarkup = GetAutoDeleteMarkup(user);

                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Settings_AutoDeleteHeaderMessage", user.LanguageCode), replyMarkup, _messageId);

                    State = BotHandlerStates.Waiting_Input_Callback;
                    OperationState = BotHandlerOperationStates.Settings_AutoDelete_Waiting;
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_Postpone)
                {
                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Settings_PostponeFormatMessage", user.LanguageCode, _minPostpone, _maxPostpone));

                    State = BotHandlerStates.Waiting_Input_Message;
                    OperationState = BotHandlerOperationStates.Settings_Postpone_Waiting;
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_DateFormat)
                {
                    var markup = GetDateFormatsMarkup(user);

                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Settings_DateFormatWelcomeMessage", user.LanguageCode), markup, _messageId);

                    State = BotHandlerStates.Waiting_Input_Callback;
                    OperationState = BotHandlerOperationStates.Settings_DateFormat_Waiting;
                    return (this, false);
                }
            }

            if (OperationState == BotHandlerOperationStates.Settings_Language_Waiting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_Language)
                {
                    if (response.Length < 2 || string.IsNullOrWhiteSpace(response[1]))
                    {
                        return (this, false);
                    }

                    var languageCode = response[1];

                    if (!SystemConstants.AvailableLanguages.ContainsKey(languageCode))
                    {
                        return (this, false);
                    }

                    var userService = ServiceProvider.GetRequiredService<IUserService>();
                    user.LanguageCode = languageCode;

                    await userService.AddOrUpdateAsync(user);

                    await SendOrEditMessage(ChatId, Localizer.GetString("Settings_LanguageSuccessMessage", user.LanguageCode));

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
            }

            if (OperationState == BotHandlerOperationStates.Settings_Location_Waiting)
            {
                if (update.Message?.Location == null)
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Settings_LocationInvalidLocationDataMessage", user.LanguageCode), new ReplyKeyboardRemove());

                    return (this, false);
                }

                var locationService = ServiceProvider.GetRequiredService<ILocationService>();
                var userTimeZone = locationService.GetUserTimeZone(update.Message.Location.Latitude, update.Message.Location.Longitude);

                if (userTimeZone == null)
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Settings_LocationInvalidLocationMessage", user.LanguageCode), new ReplyKeyboardRemove());

                    return (this, false);
                }

                var culture = GetCultureInfo(user.LanguageCode, userTimeZone.CountryCode!);
                var countryEmoji = userTimeZone.CountryCode!.IsoCountryCodeToFlagEmoji();
                var userService = ServiceProvider.GetRequiredService<IUserService>();

                user.TelegramUserSettings.Longitude = update.Message.Location.Longitude;
                user.TelegramUserSettings.Latitude = update.Message.Location.Latitude;
                user.TelegramUserSettings.TimeZoneId = userTimeZone.TimeZoneId;
                user.TelegramUserSettings.CountryName = userTimeZone.CountryName;
                user.TelegramUserSettings.CountryCode = userTimeZone.CountryCode;
                user.TelegramUserSettings.DateTimeFormat = culture.GetDateTimeFormat();
                user.TelegramUserSettings.DateTimeFormatType = SystemConstants.LocationTypeFormat;

                await userService.AddOrUpdateAsync(user);

                var formattedResult = Localizer.GetString("Settings_LocationSuccessMessage", user.LanguageCode, countryEmoji,
                   userTimeZone.TimeZoneId, userTimeZone.TimeZoneInfo.DisplayName, GetFormattedDate(DateTime.Now, user));

                await SendOrEditMessage(ChatId, formattedResult, new ReplyKeyboardRemove());

                SetIdlingState();
                return (new IdleHandler(), false);
            }

            if (OperationState == BotHandlerOperationStates.Settings_AutoDelete_Waiting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_AutoDelete)
                {
                    var userService = ServiceProvider.GetRequiredService<IUserService>();

                    user.TelegramUserSettings.IsAutoDeleteMessages = !user.TelegramUserSettings.IsAutoDeleteMessages;

                    await userService.AddOrUpdateAsync(user);

                    var messageOption = user.TelegramUserSettings.IsAutoDeleteMessages
                       ? Localizer.GetString("Settings_AutoDeleteMenu_Enabled", user.LanguageCode).ToLower()
                       : Localizer.GetString("Settings_AutoDeleteMenu_Disabled", user.LanguageCode).ToLower();
                    var message = Localizer.GetString("Settings_AutoDeleteSuccessFormatMessage", user.LanguageCode, messageOption);

                    await DeleteMessage(ChatId, _messageId!.Value);
                    await SendOrEditMessage(ChatId, message);

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
            }

            if (OperationState == BotHandlerOperationStates.Settings_Postpone_Waiting)
            {
                var message = update.GetMessage();

                if (string.IsNullOrWhiteSpace(message))
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Settings_PostponeInvalidFormatMessage", user.LanguageCode, _minPostpone, _maxPostpone));
                    return (this, false);
                }

                if (!int.TryParse(message, out var result) || (result < _minPostpone || result > _maxPostpone))
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Settings_PostponeInvalidFormatMessage", user.LanguageCode, _minPostpone, _maxPostpone));
                    return (this, false);
                }

                var userService = ServiceProvider.GetRequiredService<IUserService>();

                user.TelegramUserSettings.PostponeMinutes = result;

                await userService.AddOrUpdateAsync(user);

                await SendOrEditMessage(ChatId, Localizer.GetString("Settings_PostponeSuccessFormatMessage", user.LanguageCode, result));

                SetIdlingState();
                return (new IdleHandler(), false);
            }

            if (OperationState == BotHandlerOperationStates.Settings_DateFormat_Waiting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (result == BotCallbackArgs.SettingsHandler_DateFormatValue)
                {
                    if (response.Length < 2 || string.IsNullOrWhiteSpace(response[1]))
                    {
                        return (this, false);
                    }

                    var typeFormat = response[1];

                    if (!SystemConstants.DateTimeFormatNames.ContainsKey(typeFormat))
                    {
                        return (this, false);
                    }

                    var userService = ServiceProvider.GetRequiredService<IUserService>();

                    user.TelegramUserSettings.DateTimeFormatType = typeFormat;

                    if (typeFormat == SystemConstants.LocationTypeFormat)
                    {
                        var culture = GetCultureInfo(user.LanguageCode, user.TelegramUserSettings.CountryCode!);
                        user.TelegramUserSettings.DateTimeFormat = culture.GetDateTimeFormat();
                    }
                    else
                    {
                        user.TelegramUserSettings.DateTimeFormat = SystemConstants.DateTimeFormats[typeFormat].Format!;
                    }

                    await userService.AddOrUpdateAsync(user);
                    await DeleteMessage(ChatId, _messageId!.Value);
                    await SendOrEditMessage(ChatId, Localizer.GetString("Settings_DateFormatSuccessFormatMessage", user.LanguageCode, SystemConstants.DateTimeFormatNames[typeFormat]));

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
            }

            return (this, false);
        }

        private InlineKeyboardMarkup GetSettingsMarkup(TelegramUserModel user)
        {
            var userSettings = user.TelegramUserSettings;
            var languageCode = user.LanguageCode;
            var minutes = userSettings.PostponeMinutes;
            var autoDeleteText = userSettings.IsAutoDeleteMessages
                ? Localizer.GetString("Settings_AutoDeleteMenu_Enabled", languageCode)
                : Localizer.GetString("Settings_AutoDeleteMenu_Disabled", languageCode);

            return new InlineKeyboardMarkup(
              new[]
              {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Settings_SetLanguageMenuButton", languageCode),
                        MakeCommandArgs(BotCallbackArgs.SettingsHandler_Language)),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Settings_SetLocationMenuFormatButton", languageCode, userSettings.TimeZoneId ?? "?"),
                        MakeCommandArgs(BotCallbackArgs.SettingsHandler_Location)),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Settings_AutoDeleteMenuFormatButton", languageCode, autoDeleteText),
                        MakeCommandArgs(BotCallbackArgs.SettingsHandler_AutoDelete)),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Settings_PostponeMenuFormatButton", languageCode, minutes),
                        MakeCommandArgs(BotCallbackArgs.SettingsHandler_Postpone)),
                    },
                    !user.IsLocationSet
                    ? Array.Empty<InlineKeyboardButton>()
                    : new []
                      {
                          InlineKeyboardButton.WithCallbackData(Localizer.GetString("Settings_DateFormatMenuButton", languageCode),
                          MakeCommandArgs(BotCallbackArgs.SettingsHandler_DateFormat)),
                      }
              });
        }

        private InlineKeyboardMarkup GetLanguageMarkup()
        {
            var content = new List<List<InlineKeyboardButton>>();

            foreach (var langugage in SystemConstants.AvailableLanguages)
            {
                content.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData(langugage.Value, MakeCommandArgs(BotCallbackArgs.SettingsHandler_Language, langugage.Key)) });
            }

            return new InlineKeyboardMarkup(content);
        }

        private ReplyKeyboardMarkup GetShareLocationMarkup(string languageCode)
        {
            return new ReplyKeyboardMarkup(new[]
                { KeyboardButton.WithRequestLocation(Localizer.GetString("Settings_LocationShareButton", languageCode)) })
                { ResizeKeyboard = true };
        }

        private InlineKeyboardMarkup GetAutoDeleteMarkup(TelegramUserModel user)
        {
            var messageOption = user.TelegramUserSettings.IsAutoDeleteMessages
                        ? Localizer.GetString("Settings_AutoDelete_Disable", user.LanguageCode)
                        : Localizer.GetString("Settings_AutoDelete_Enable", user.LanguageCode);
            var message = Localizer.GetString("Settings_AutoDeleteFormatButton", user.LanguageCode, messageOption);

            return new InlineKeyboardMarkup(
              new[]
              {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(message, MakeCommandArgs(BotCallbackArgs.SettingsHandler_AutoDelete)),
                    }
              });
        }

        private InlineKeyboardMarkup GetDateFormatsMarkup(TelegramUserModel user)
        {
            var userSettings = user.TelegramUserSettings;

            var culture = GetCultureInfo(user.LanguageCode, user.TelegramUserSettings.CountryCode!);
            var formatType = userSettings.DateTimeFormatType;
            var formatName = SystemConstants.DateTimeFormatNames[formatType];
            var currentText = Localizer.GetString("Settings_DateFormatCurrentFormatButton", user.LanguageCode, formatName);
            var locationFormatText = $"{SystemConstants.DateTimeFormatNames[SystemConstants.LocationTypeFormat]} : {_exampleDatetime.ToString(culture.GetDateTimeFormat(), culture)}";

            var formatButtons = GetDateFormatButtons(currentText, formatType, locationFormatText);

            var content = new List<List<InlineKeyboardButton>>();

            foreach (var button in formatButtons)
            {
                content.Add(new List<InlineKeyboardButton>() { button });
            }

            return new InlineKeyboardMarkup(content);
        }

        private List<InlineKeyboardButton> GetDateFormatButtons(string currentFormatText, string formatType, string locationFormatText)
        {
            var builder = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text: currentFormatText,
                            callbackData: MakeCommandArgs(BotCallbackArgs.SettingsHandler_DateFormatValue, formatType))
            };

            foreach (var format in SystemConstants.DateTimeFormats)
            {
                var text = $"{SystemConstants.DateTimeFormatNames[format.Key]} : {_exampleDatetime.ToString(format.Value.Format, format.Value.Culture)}";
                builder.Add(InlineKeyboardButton.WithCallbackData(text: text,
                    callbackData: MakeCommandArgs(BotCallbackArgs.SettingsHandler_DateFormatValue, format.Key)));
            }

            builder.Add(InlineKeyboardButton.WithCallbackData(text: locationFormatText,
                            callbackData: MakeCommandArgs(BotCallbackArgs.SettingsHandler_DateFormatValue, SystemConstants.LocationTypeFormat)));

            return builder;
        }
    }
}
