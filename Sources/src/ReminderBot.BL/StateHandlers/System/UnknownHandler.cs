using System;
using System.Threading.Tasks;
using ReminderBot.BL.StateHandlers.Reminders;
using ReminderBot.Models.Enums;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class UnknownHandler : BaseMessageHandler
    {
        public UnknownHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => string.Empty;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var message = update.GetMessage();
            var isUnknownCommand = message != null && message.StartsWith('/');

            if (isUnknownCommand)
            {
                await SendOrEditMessage(ChatId, await GetLocalizedStringAsync("Unknown_Message"));

                SetIdlingState();
                return (new IdleHandler(), false);
            }
            else
            {
                SetIdlingState();
                return (new CreateHandler(ServiceProvider), true);
            }
        }
    }
}
