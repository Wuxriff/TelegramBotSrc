using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers.System
{
    public class StopHandler : BaseMessageHandler
    {
        public StopHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandStop;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = (await GetUser())!;

            if (State == BotHandlerStates.Starting)
            {
                var replyMarkup = GetKeyboardMarkup(user.LanguageCode);

                await SendOrEditMessage(ChatId, Localizer.GetString("Stop_StopMessage", user.LanguageCode), replyMarkup);

                State = BotHandlerStates.Waiting_Input_Callback;
                OperationState = BotHandlerOperationStates.Stop_Waiting;
                return (this, false);
            }

            if (OperationState == BotHandlerOperationStates.Stop_Waiting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                user.IsActive = false;
                user.TelegramUserSettings.IsPaused = true;

                var userService = ServiceProvider.GetRequiredService<IUserService>();

                if (result == BotCallbackArgs.StopHandler_Stop)
                {
                    await userService.AddOrUpdateAsync(user);
                }
                else if (result == BotCallbackArgs.StopHandler_Stop_Erase)
                {
                    await userService.DeleteAsync(user.UserId);
                }
                else
                {
                    return (this, false);
                }

                await SendOrEditMessage(ChatId, Localizer.GetString("Stop_StoppedFormatMessage", user.LanguageCode, CommandConstants.TelegramBotCommandStart));

                SetIdlingState();
                return (new IdleHandler(), false);
            }

            return (this, false);
        }

        private InlineKeyboardMarkup GetKeyboardMarkup(string locCode)
        {
            return new InlineKeyboardMarkup(
               new[]
               {
                   new []
                   {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Stop_StopButton", locCode), MakeCommandArgs(BotCallbackArgs.StopHandler_Stop)),
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Stop_StopEraseButton", locCode), MakeCommandArgs(BotCallbackArgs.StopHandler_Stop_Erase)),
                   }
               });
        }
    }
}
