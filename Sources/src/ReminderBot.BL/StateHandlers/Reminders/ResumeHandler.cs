using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers.System;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.Reminders
{
    public class ResumeHandler : BaseMessageHandler
    {
        public ResumeHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandResume;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var userService = ServiceProvider.GetRequiredService<IUserService>();

            var user = await GetUser();

            user.TelegramUserSettings.IsPaused = false;

            await userService.AddOrUpdateAsync(user);

            await SendOrEditMessage(ChatId, Localizer.GetString("Resume_RemindersResumedMessage", user.LanguageCode));

            SetIdlingState();
            return (new IdleHandler(), false);
        }
    }
}
