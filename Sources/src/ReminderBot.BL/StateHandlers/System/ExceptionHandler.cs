using System;
using System.Threading.Tasks;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers.System
{
    public class ExceptionHandler : BaseMessageHandler
    {
        public ExceptionHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandException;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = await GetUser();
            await SendOrEditMessage(ChatId, Localizer.GetString("Exception_Message", user.LanguageCode), markup: new ReplyKeyboardRemove());

            SetIdlingState();
            return (new IdleHandler(), false);
        }
    }
}