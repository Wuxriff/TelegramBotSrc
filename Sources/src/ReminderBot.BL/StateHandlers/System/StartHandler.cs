using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Enums;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;

namespace ReminderBot.BL.StateHandlers.System
{
    public class StartHandler : BaseMessageHandler
    {
        public StartHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandStart;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var userService = ServiceProvider.GetRequiredService<IUserService>();

            var user = update.GetUser();

            var isRegistered = await userService.IsRegisteredAsync(UserId);

            TelegramUserModel? userModel;

            if (isRegistered)
            {
                userModel = await userService.GetAsync(UserId, false);
            }
            else
            {
                userModel = new TelegramUserModel
                {
                    TelegramUserSettings = new TelegramUserSettingsModel()
                };
            }

            userModel.IsActive = true;
            userModel.UserId = UserId;
            userModel.ChatId = ChatId;
            userModel.UserName = user.Username!;
            userModel.LanguageCode = user.LanguageCode!;

            userModel.TelegramUserSettings.PostponeMinutes = 5;
            userModel.TelegramUserSettings.IsAutoDeleteMessages = false;
            userModel.TelegramUserSettings.IsPaused = false;
            userModel.TelegramUserSettings.DateTimeFormat = SystemConstants.DateTimeFormats[SystemConstants.UniTypeFormat].Culture.GetDateTimeFormat();
            userModel.TelegramUserSettings.DateTimeFormatType = SystemConstants.UniTypeFormat;

            await userService.AddOrUpdateAsync(userModel);

            var message = Localizer.GetString("Start_WelcomeFormatMessage", user.LanguageCode!, userModel.UserName);

            await SendOrEditMessage(ChatId, message);

            SetIdlingState();
            return (new HelpHandler(ServiceProvider), true);
        }
    }
}
