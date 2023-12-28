using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers.System;
using ReminderBot.Models.Enums;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL.StateHandlers.Reminders
{
    public class ManageHandler : BaseMessageHandler
    {
        private const int _maxPageButtons = 5;
        private const int _maxRemindersPerPage = 5;

        private int _currentPage = 1;
        private int _previousPage = 0;
        private int? _messageId;

        public ManageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            State = BotHandlerStates.Starting;
            OperationState = BotHandlerOperationStates.None;
        }

        public override string Command => CommandConstants.TelegramBotCommandManage;

        protected override async Task<(BaseStateHandler Handler, bool HandleNext)> HandleAsync(Update update)
        {
            var user = await GetUser();

            if (State == BotHandlerStates.Starting)
            {
                var replyMarkup = GetManageMarkup(user.LanguageCode);

                _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Manage_MainHeaderMessage", user.LanguageCode), replyMarkup, _messageId);

                State = BotHandlerStates.Waiting_Input_Callback;
                OperationState = BotHandlerOperationStates.Manage_Waiting;
                return (this, false);
            }

            if (OperationState == BotHandlerOperationStates.Manage_Waiting)
            {
                var response = update.GetCallbackArgs();

                if (!TryParseCallbackArgs(response, out var result))
                {
                    return (this, false);
                }

                if (result == BotCallbackArgs.ViewHandler_ViewActive)
                {
                    var reminderService = ServiceProvider.GetRequiredService<IReminderService>();
                    var remindersCount = await reminderService.GetUserRemindersCountAsync(UserId, false);
                    var totalPages = (remindersCount / _maxRemindersPerPage) + (_maxRemindersPerPage < 2 ? 0 : 1);

                    var skip = (_currentPage - 1) * _maxRemindersPerPage;

                    var reminders = await reminderService.GetUserRemindersAsync(UserId, false, true, skip, _maxRemindersPerPage, true);

                    var pageText = GetReminderButtons(reminders, user);
                    var keyboard = GetPaginationButtons(totalPages, _currentPage);
                    var content = Getcontent(pageText, keyboard);

                    _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Manage_WelcomeMessage", user.LanguageCode), content, _messageId);

                    _previousPage = _currentPage;

                    OperationState = BotHandlerOperationStates.Manage_View_Waiting;
                    State = BotHandlerStates.Waiting_Input_Callback;
                    return (this, false);
                }

                if (result == BotCallbackArgs.ViewHandler_ExportAll)
                {
                    var reminderService = ServiceProvider.GetRequiredService<IReminderService>();
                    var reminders = await reminderService.GetUserRemindersAsync(UserId, true, false);
                    var content = GetExportContent(reminders, user);
                    var byteArray = Encoding.UTF8.GetBytes(content);

                    using var memoryStream = new MemoryStream(byteArray);
                    var inputStream = new InputFileStream(memoryStream, fileName: "reminders.txt");
                    await BotContext.BotClient.SendDocumentAsync(ChatId, inputStream);

                    SetIdlingState();
                    return (new IdleHandler(), false);
                }
            }

            if (OperationState == BotHandlerOperationStates.Manage_View_Waiting)
            {
                var response = update.GetCallbackArgs();

                var reminderService = ServiceProvider.GetRequiredService<IReminderService>();
                var remindersCount = await reminderService.GetUserRemindersCountAsync(UserId, false);
                var totalPages = (remindersCount / _maxRemindersPerPage) + (_maxRemindersPerPage < 2 ? 0 : 1);

                if (response.Any())
                {
                    if (!TryParseCallbackArgs(response, out var result))
                    {
                        return (this, false);
                    }

                    if (response.Length < 2 || !int.TryParse(response[1], out _currentPage))
                    {
                        return (this, false);
                    }

                    _currentPage = result switch
                    {
                        BotCallbackArgs.ViewHandler_First => 1,
                        BotCallbackArgs.ViewHandler_Prev => _currentPage - 1,
                        BotCallbackArgs.ViewHandler_Next => _currentPage + 1,
                        BotCallbackArgs.ViewHandler_Last => totalPages,
                        _ => _currentPage
                    };
                }
                else
                {
                    _messageId = null;
                }

                if (_previousPage == _currentPage)
                {
                    _currentPage = 0;
                    return (this, false);
                }

                var skip = (_currentPage - 1) * _maxRemindersPerPage;

                var reminders = await reminderService.GetUserRemindersAsync(UserId, false, true, skip, _maxRemindersPerPage, true);

                var pageText = GetReminderButtons(reminders, user);
                var keyboard = GetPaginationButtons(totalPages, _currentPage);
                var content = Getcontent(pageText, keyboard);

                _messageId = await SendOrEditMessage(ChatId, Localizer.GetString("Manage_WelcomeMessage", user.LanguageCode), content, _messageId);

                _previousPage = _currentPage;

                OperationState = BotHandlerOperationStates.Manage_View_Waiting;
                State = BotHandlerStates.Waiting_Input_Callback;
                return (this, false);
            }

            return (this, false);
        }

        private InlineKeyboardMarkup GetManageMarkup(string languageCode)
        {
            return new InlineKeyboardMarkup(
              new[]
              {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Manage_ViewActiveButton", languageCode),
                        MakeCommandArgs(BotCallbackArgs.ViewHandler_ViewActive)),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(Localizer.GetString("Manage_ExportAllButton", languageCode),
                        MakeCommandArgs(BotCallbackArgs.ViewHandler_ExportAll)),
                    },
              });
        }

        private List<InlineKeyboardButton> GetPaginationButtons(int totalPages, int currentPage)
        {
            var buttons = new List<InlineKeyboardButton>();

            var prevPos = currentPage - 1;
            var nextPos = currentPage + 1;

            if (totalPages <= _maxPageButtons)
            {
                for (int i = 1; i <= totalPages; i++)
                {
                    if (currentPage == i)
                    {
                        buttons.Add(InlineKeyboardButton.WithCallbackData("·" + i + "·", MakeCommandArgs(BotCallbackArgs.ViewHandler_GoTo, i)));
                        continue;
                    }

                    buttons.Add(InlineKeyboardButton.WithCallbackData(i.ToString(), MakeCommandArgs(BotCallbackArgs.ViewHandler_GoTo, i)));
                }
            }
            else
            {
                if (prevPos > 1)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData("<< " + 1, MakeCommandArgs(BotCallbackArgs.ViewHandler_First)));
                }

                for (int i = 1; i <= totalPages; i++)
                {
                    if (currentPage == i)
                    {
                        buttons.Add(InlineKeyboardButton.WithCallbackData("·" + i + "·", MakeCommandArgs(BotCallbackArgs.ViewHandler_GoTo, i)));
                        continue;
                    }

                    if (prevPos == i)
                    {
                        if (prevPos == 1)
                        {
                            buttons.Add(InlineKeyboardButton.WithCallbackData(i.ToString(), MakeCommandArgs(BotCallbackArgs.ViewHandler_GoTo, i)));
                        }
                        else
                        {
                            buttons.Add(InlineKeyboardButton.WithCallbackData("< " + i, MakeCommandArgs(BotCallbackArgs.ViewHandler_Prev)));
                            continue;
                        }
                    }

                    if (nextPos == i)
                    {
                        if (nextPos == totalPages)
                        {
                            buttons.Add(InlineKeyboardButton.WithCallbackData(i.ToString(), MakeCommandArgs(BotCallbackArgs.ViewHandler_GoTo, i)));
                        }
                        else
                        {
                            buttons.Add(InlineKeyboardButton.WithCallbackData(i + " >", MakeCommandArgs(BotCallbackArgs.ViewHandler_Next)));
                            continue;
                        }
                    }
                }

                if (nextPos < totalPages)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(totalPages + " >>", MakeCommandArgs(BotCallbackArgs.ViewHandler_Last)));
                }
            }

            return buttons;
        }

        private List<InlineKeyboardButton> GetReminderButtons(List<ReminderModel> reminders, TelegramUserModel user)
        {
            var builder = new List<InlineKeyboardButton>();

            foreach (var reminder in reminders)
            {
                var text = string.Format("{0} | {1}", GetFormattedDate(reminder.ReminderDate, user), reminder.Content);

                builder.Add(InlineKeyboardButton.WithCallbackData(text: text, callbackData: MakeCommandArgs(CommandConstants.TelegramBotCommandEdit, BotCallbackArgs.EditHandler_View, reminder.Id)));
            }

            return builder;
        }

        private InlineKeyboardMarkup Getcontent(List<InlineKeyboardButton> reminders, List<InlineKeyboardButton> paginationButtons)
        {
            var content = new List<List<InlineKeyboardButton>>();

            foreach (var reminder in reminders)
            {
                content.Add(new List<InlineKeyboardButton>() { reminder });
            }

            content.Add(paginationButtons);

            return new InlineKeyboardMarkup(content);
        }

        private string GetExportContent(List<ReminderModel> reminders, TelegramUserModel user)
        {
            if (!reminders.Any())
            {
                return Localizer.GetString("Manage_ExportEmptyMessage", user.LanguageCode);
            }
            else
            {
                var builder = new StringBuilder();

                var groupedReminders = reminders.GroupBy(x => new { x.ReminderDate.Year, x.ReminderDate.Month }).OrderByDescending(x => x.Key.Year).ThenByDescending(x => x.Key.Month);

                foreach (var reminderGroup in groupedReminders)
                {
                    builder.AppendFormat("{0} | {1}", reminderGroup.Key.Year, reminderGroup.Key.Month).AppendLine();

                    foreach (var reminder in reminderGroup)
                    {
                        builder.AppendFormat("{0} | {1}", GetFormattedDate(reminder.ReminderDate, user), reminder.Content).AppendLine();
                    }

                    builder.AppendLine();
                }

                return builder.ToString();
            }
        }
    }
}
