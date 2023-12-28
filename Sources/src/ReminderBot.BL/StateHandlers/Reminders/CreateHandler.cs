using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers.System;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers.Reminders
{
    public class CreateHandler : BaseMessageHandler
    {
        private DateTime _parsedDate;
        private string? _parsedText;
        private int? _messageId;

        public CreateHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        private readonly static string[] _dateTimeformats = new string[]
        {
            "dd/MM/yyyy HH:mm",
            "dd/MM/yy HH:mm",
            "dd.MM.yyyy HH:mm",
            "dd.MM.yy HH:mm",

            "dd/MM HH:mm",
            "dd/MM HH:mm",
            "dd.MM HH:mm",
            "dd.MM HH:mm",

            "dd/MM/yyyy HH:mm",
            "dd/MM/yy HH:mm",
            "dd.MM.yyyy HH:mm",
            "dd.MM.yy HH:mm",

            "dd/MM HH:mm",
            "dd/MM HH:mm",
            "dd.MM HH:mm",
            "dd.MM HH:mm",

            "dd HH:mm",
            "dd HH:mm",
            "dd HH:mm",
            "dd HH:mm",
        };

        public override string Command => CommandConstants.TelegramBotCommandCreate;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = await GetUser();

            if (State == BotHandlerStates.Starting)
            {
                if (!user.IsLocationSet)
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Create_NoLocationFormatError", user.LanguageCode, CommandConstants.TelegramBotCommandSettings));

                    SetIdlingState();
                    return (new SettingsHandler(ServiceProvider), false);
                }

                var message = update.GetMessage();

                if (!TryParseDate(message, out _parsedDate))
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Create_InvalidDateFormatError", user.LanguageCode, CommandConstants.TelegramBotCommandHelp));

                    return (this, false);
                }

                var userMessageArray = message?.ReadLines()?.Skip(1).ToList();
                _parsedText = userMessageArray?.Any() == null ? string.Empty : string.Join(Environment.NewLine, userMessageArray);

                if (string.IsNullOrWhiteSpace(_parsedText))
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Create_InvalidTextError", user.LanguageCode));

                    ResetState();
                    return (this, false);
                }

                var markup = GetViewDateMarkup(user.LanguageCode);

                _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Create_HeaderParsedDateFormatMessage", user.LanguageCode,
                    GetFormattedDate(_parsedDate, user)), markup);

                State = BotHandlerStates.Waiting_Input_Callback;
                OperationState = BotHandlerOperationStates.Create_Waiting_Date_Confirm;
                return (this, false);
            }

            if (OperationState == BotHandlerOperationStates.Create_Waiting_Date_Confirm)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (result == BotCallbackArgs.CreateHandler_Date_Correct)
                {
                    var reminderService = ServiceProvider.GetRequiredService<IReminderService>();

                    await reminderService.AddAsync(update!.GetUserId(), _parsedText!, _parsedDate);
                    await SendOrEditMessage(ChatId, Localizer.GetString("Create_SuccessFormatMessage", user.LanguageCode,
                        GetFormattedDate(_parsedDate, user)), messageId: _messageId);

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
                else if (result == BotCallbackArgs.CreateHandler_Date_Incorrect)
                {
                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Create_HeaderParsedIncorrectMessage", user.LanguageCode), messageId: _messageId);
                    _parsedDate = default;
                    _parsedText = null;

                    ResetState();
                    return (this, false);
                }
            }

            return (this, false);
        }

        private bool TryParseDate(string? message, out DateTime parsedResult)
        {
            var result = false;

            var dateRow = message?.ReadLines().FirstOrDefault();

            if (DateTime.TryParse(dateRow, SystemConstants.DateTimeFormats[SystemConstants.EuTypeFormat].Culture, out parsedResult)
                || DateTime.TryParse(dateRow, SystemConstants.DateTimeFormats[SystemConstants.UniTypeFormat].Culture, out parsedResult)
                || DateTime.TryParseExact(dateRow, _dateTimeformats, SystemConstants.DateTimeFormats[SystemConstants.EuTypeFormat].Culture, DateTimeStyles.None, out parsedResult))
            {
                result = true;
            }

            if (result)
            {
                var now = DateTime.Now;

                if (parsedResult < now)
                {
                    if (parsedResult.Year < now.Year)
                    {
                        parsedResult = new DateTime(now.Year, parsedResult.Month, parsedResult.Day, parsedResult.Hour, parsedResult.Minute, parsedResult.Second);
                    }

                    if (parsedResult.Month <= now.Month)
                    {
                        if (parsedResult.Day > now.Day)
                        {
                            parsedResult = new DateTime(now.Year, now.Month, parsedResult.Day, parsedResult.Hour, parsedResult.Minute, parsedResult.Second);
                        }
                        else
                        {
                            parsedResult = new DateTime(now.Year, now.Month + 1, parsedResult.Day, parsedResult.Hour, parsedResult.Minute, parsedResult.Second);
                        }
                    }
                }
            }

            return result;
        }


        private InlineKeyboardMarkup GetViewDateMarkup(string languageCode)
        {
            return new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: Localizer.GetString("Create_YesButton", languageCode), callbackData: MakeCommandArgs(BotCallbackArgs.CreateHandler_Date_Correct)),
                InlineKeyboardButton.WithCallbackData(text: Localizer.GetString("Create_NoButton", languageCode), callbackData: MakeCommandArgs(BotCallbackArgs.CreateHandler_Date_Incorrect))
            });
        }
    }
}
