using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers.System;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.Reminders
{
    public class ReminderHandler : BaseMessageHandler
    {
        private const string ReminderWrapper = "\U000023F0 {0}";

        public ReminderHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandReminder;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var reminderService = ServiceProvider.GetRequiredService<IReminderService>();

            if (State == BotHandlerStates.Starting)
            {
                var response = update.GetCallbackArgs();
                int reminderId = 0;

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (response.Length < 2 || !int.TryParse(response[1], out reminderId))
                {
                    return (this, false);
                }

                var reminder = await reminderService.GetAsync(reminderId);

                if (reminder.TelegramUser.UserId != UserId)
                {
                    var user = await GetUser();
                    await SendOrEditMessage(ChatId, Localizer.GetString("Reminder_InvalidUserMessage", user.LanguageCode));

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }

                if (result == BotCallbackArgs.ReminderHandler_Postpone)
                {
                    await reminderService.PostponeAsync(reminder, reminder.TelegramUser.TelegramUserSettings.PostponeMinutes);

                    await DeleteMessage(ChatId, reminder.MessageId!.Value);

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }

                if (result == BotCallbackArgs.ReminderHandler_Confirm)
                {
                    var messageId = reminder.MessageId!.Value;
                    var isKeep = !reminder.TelegramUser.TelegramUserSettings.IsAutoDeleteMessages;

                    await reminderService.ConfirmAsync(reminder);

                    if (isKeep)
                    {
                        var message = string.Format(ReminderWrapper, reminder.Content);
                        await SendOrEditMessage(ChatId, message, messageId: messageId);
                    }
                    else
                    {
                        await DeleteMessage(ChatId, messageId);
                    }

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
            }

            return (this, false);
        }
    }
}
