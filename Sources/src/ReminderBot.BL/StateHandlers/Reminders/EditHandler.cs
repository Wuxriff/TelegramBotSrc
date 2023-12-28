using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers.System;
using ReminderBot.Models.Enums;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers.Reminders
{
    public class EditHandler : BaseMessageHandler
    {
        private int? _messageId;

        public EditHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandEdit;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var reminderService = ServiceProvider.GetRequiredService<IReminderService>();

            if (State == BotHandlerStates.Starting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (response.Length < 2 || !int.TryParse(response[1], out int reminderId))
                {
                    return (this, false);
                }

                var reminder = await reminderService.GetAsync(reminderId);
                var user = await GetUser();

                if (reminder.TelegramUser.UserId != UserId)
                {
                    await SendOrEditMessage(ChatId, Localizer.GetString("Edit_InvalidUserMessage", user.LanguageCode));

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }

                // Redirected from ViewHandler
                if (result == BotCallbackArgs.EditHandler_View)
                {
                    var reminderContent = GetReminderContent(reminder, user.LanguageCode, user.TelegramUserSettings.DateTimeFormat);
                    var markup = GetViewReminderMarkup(reminder, user.LanguageCode);

                    _messageId = await SendOrEditMessage(ChatId, reminderContent, markup, _messageId);

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }

                // Redirected from ViewHandler
                if (result == BotCallbackArgs.EditHandler_Delete)
                {
                    await reminderService.DeleteAsync(reminderId);

                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Edit_ReminderDeletedMessage", user.LanguageCode), messageId: _messageId);

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
            }

            return (this, false);
        }

        private string GetReminderContent(ReminderModel reminder, string languageCode, string formatter)
        {
            return Localizer.GetString("Edit_ShowReminderContentFormatMessage", languageCode, reminder.ReminderDate.ToString(formatter), reminder.Content);
        }

        private InlineKeyboardMarkup GetViewReminderMarkup(ReminderModel reminder, string languageCode)
        {
            return new InlineKeyboardMarkup(new[]
            {
                //InlineKeyboardButton.WithCallbackData(text: "Edit", callbackData: MakeCommandArgs(BotCallbackArgs.EditHandler_Edit)),
                InlineKeyboardButton.WithCallbackData(text: Localizer.GetString("Edit_DeleteReminderButton", languageCode), callbackData: MakeCommandArgs(BotCallbackArgs.EditHandler_Delete, reminder.Id))
            });
        }
    }
}
