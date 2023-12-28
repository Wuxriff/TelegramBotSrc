using System;
using System.Text;
using System.Threading.Tasks;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class HelpHandler : BaseMessageHandler
    {
        public HelpHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandHelp;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = await GetUser();
            await SendOrEditMessage(ChatId, GetHelpContent(user.LanguageCode));

            SetIdlingState();
            return (new IdleHandler(), false);
        }

        private string GetHelpContent(string locCode)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Localizer.GetString("Help_HeaderMessage", locCode));
            builder.AppendFormat(Localizer.GetString("Help_SupportedLanguagesFormatMessage", locCode,
                "US".IsoCountryCodeToFlagEmoji(),
                "RU".IsoCountryCodeToFlagEmoji(),
                "BY".IsoCountryCodeToFlagEmoji(),
                "UA".IsoCountryCodeToFlagEmoji())).AppendLine();
            builder.AppendFormat(Localizer.GetString("Help_ContentFormatMessage", locCode));

            return builder.ToString();
        }
    }
}
