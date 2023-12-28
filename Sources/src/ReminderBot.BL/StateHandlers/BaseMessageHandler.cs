using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Helpers;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Enums;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers
{
    public abstract class BaseMessageHandler : BaseStateHandler
    {
        protected IServiceProvider ServiceProvider;
        protected IBotContext BotContext;
        protected ILocalizer Localizer;

        protected BaseMessageHandler(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.BotContext = serviceProvider.GetRequiredService<IBotContext>();
            this.Localizer = serviceProvider.GetRequiredService<ILocalizer>();
        }

        protected Task<TelegramUserModel> GetUser(bool useCached = true)
        {
            var userService = ServiceProvider.GetRequiredService<IUserService>();
            return (userService.GetAsync(UserId, useCached))!;
        }

        protected async Task<int?> SendOrEditMessage(long chatId, string text, IReplyMarkup? markup = null, int? messageId = null)
        {
            if (messageId == null)
            {
                return (await BotContext.BotClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: markup)).MessageId;
            }
            else
            {
                return (await BotContext.BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId.Value, text: text, replyMarkup: (InlineKeyboardMarkup)markup!)).MessageId;
            }
        }

        protected Task<int?> SendOrEditMessage(string text, IReplyMarkup? markup = null, int? messageId = null)
        {
            return SendOrEditMessage(ChatId, text, markup, messageId);
        }

        protected Task DeleteMessage(long chatId, int messageId)
        {
            try
            {
                return BotContext.BotClient.DeleteMessageAsync(chatId, messageId);
            }
            catch
            {
                // Could be deleted by the user
            }

            return Task.CompletedTask;
        }

        protected Task DeleteMessage(int messageId)
        {
            return DeleteMessage(ChatId, messageId);
        }

        protected string MakeCommandArgs(BotCallbackArgs state, params object[] args)
        {
            return CommandHelper.MakeCommandArgs(Command, state, args);
        }

        protected string MakeCommandArgs(string command, BotCallbackArgs state, params object[] args)
        {
            return CommandHelper.MakeCommandArgs(command, state, args);
        }

        protected bool TryParseCallbackArgs(string[] args, out BotCallbackArgs result)
        {
            if (!args.Any())
            {
                result = BotCallbackArgs.None;
                return false;
            }

            return Enum.TryParse(args[0], true, out result);
        }

        protected string GetLocalizedString(string token, string languageCode)
        {
            return Localizer.GetString(token, languageCode);
        }

        protected async Task<string> GetLocalizedStringAsync(string token)
        {
            var user = await GetUser();

            return Localizer.GetString(token, user.LanguageCode);
        }


        //TODO - Cache culture and etc?
        protected string GetFormattedDate(DateTime dateTime, TelegramUserModel user)
        {
            var settings = user.TelegramUserSettings;

            CultureInfo culture;
            var formatter = settings.DateTimeFormat;

            if (settings.DateTimeFormatType == SystemConstants.LocationTypeFormat)
            {
                culture = GetCultureInfo(user.LanguageCode, settings.CountryCode!);
            }
            else
            {
                (culture, formatter) = SystemConstants.DateTimeFormats[settings.DateTimeFormatType];
            }

            return dateTime.ToString(formatter, culture);
        }

        protected CultureInfo GetCultureInfo(string languageCode, string countryCode)
        {
            return new CultureInfo(languageCode + "-" + countryCode);
        }

        protected CultureInfo GetCultureInfo(string countryCode)
        {
            return new CultureInfo(countryCode);
        }
    }
}
