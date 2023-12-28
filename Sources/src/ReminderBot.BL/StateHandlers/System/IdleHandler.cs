using System.Threading.Tasks;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class IdleHandler : BaseStateHandler
    {
        public IdleHandler()
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandIdle;

        protected override Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            SetIdlingState();
            return Task.FromResult<(BaseStateHandler Handler, bool HandleNext)>((this, false));
        }
    }
}
