using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class UnregisteredHandler : BaseMessageHandler
    {
        public UnregisteredHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandUnregistered;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var userService = ServiceProvider.GetRequiredService<IUserService>();
            var isRegistered = await userService.IsRegisteredAsync(UserId);

            string languageCode;

            if (isRegistered)
            {
                var user = await GetUser();
                languageCode = user.LanguageCode;
            }
            else
            {
                languageCode = update.GetUser().LanguageCode!;
            }

            await SendOrEditMessage(ChatId, Localizer.GetString("TelegramBot_UnregisteredUserFormatMessage", languageCode, CommandConstants.TelegramBotCommandStart));

            SetIdlingState();
            return (new IdleHandler(), false);
        }
    }
}
