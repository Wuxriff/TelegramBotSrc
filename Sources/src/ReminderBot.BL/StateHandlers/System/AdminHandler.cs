using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class AdminHandler : BaseMessageHandler
    {
        private readonly IBotConfigurationService _configurationService;

        public AdminHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configurationService = serviceProvider.GetRequiredService<IBotConfigurationService>();

            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandAdmin;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            if (State == BotHandlerStates.Starting)
            {
                if (!IsAdmin())
                {
                    var user = await GetUser();
                    await SendOrEditMessage(ChatId, Localizer.GetString("Admin_NotAdminMessage", user.LanguageCode));

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }

                //Show markup keyboard

                State = BotHandlerStates.Waiting_Input_Callback;
                OperationState = BotHandlerOperationStates.Admin_Waiting;
                return (this, false);
            }

            if (OperationState == BotHandlerOperationStates.Admin_Waiting)
            {
                // Do smth
            }

            SetIdlingState();
            return (this, false);
        }

        private bool IsAdmin()
        {
            return _configurationService.GetAdminTelegramUserId() == UserId;
        }
    }
}