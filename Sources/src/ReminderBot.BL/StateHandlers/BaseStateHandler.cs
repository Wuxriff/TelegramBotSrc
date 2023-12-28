using System.Threading.Tasks;
using ReminderBot.Models.Enums;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers
{
    public abstract class BaseStateHandler
    {
        protected long ChatId;
        protected long UserId;

        public BotHandlerStates State { get; protected set; }
        protected BotHandlerOperationStates OperationState { get; set; }
        public abstract string Command { get; }
        protected abstract Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update);

        protected virtual void ResetState()
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        protected void SetIdlingState()
        {
            State = BotHandlerStates.Idling;
            OperationState = BotHandlerOperationStates.None;
        }

        public Task<(BaseStateHandler Handler, bool HandleNext)> HandleUpdateAsync(Update update)
        {
            this.ChatId = update.GetChatId();
            this.UserId = update.GetUserId();

            return HandleAsync(update);
        }
    }
}
