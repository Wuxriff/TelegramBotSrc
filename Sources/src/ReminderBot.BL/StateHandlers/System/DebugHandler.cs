using System;
using System.Threading.Tasks;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class DebugHandler : BaseMessageHandler
    {
        public DebugHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandDebug;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = await GetUser();
            await SendOrEditMessage(ChatId, $"TelegramUserId={user.UserId}");

            SetIdlingState();
            return (new IdleHandler(), false);
        }
    }
}