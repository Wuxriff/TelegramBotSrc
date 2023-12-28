using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ReminderBot.Shared.Extensions
{
    public static class UpdateExtensions
    {
        public static long GetUserId(this Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                return update.Message!.From!.Id;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery!.From!.Id;
            }

            throw new Exception("Failed to get UserId");
        }

        public static long GetChatId(this Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                return update.Message!.Chat!.Id;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery!.Message!.Chat!.Id;
            }

            throw new Exception("Failed to get UserId");
        }

        public static User GetUser(this Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                return update.Message!.From!;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery!.Message!.From!;
            }

            throw new Exception("Failed to get User");
        }

        public static string GetCallbackQueryId(this Update update)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery!.Id;
            }

            throw new Exception("Failed to get CallbackQueryId");
        }

        public static string[] GetCallbackArgs(this Update update)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery!.Data!.Split(CommandConstants.CommandSeparator).Skip(1).ToArray();
            }

            return Array.Empty<string>();
        }

        public static string? GetCommand(this Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                return update.GetMessage();
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery!.Data!.Split(CommandConstants.CommandSeparator)[0];
            }

            throw new Exception("Failed to get command");
        }

        public static string? GetMessage(this Update update)
        {
            return update?.Message?.Text?.Trim();
        }

        public static bool IsCallbackCommand(this Update update, string command) =>
            update.CallbackQuery!.Data!.StartsWith(command, StringComparison.Ordinal);

        public static string TrimCallbackCommand(this Update update, string pattern) =>
           update.CallbackQuery!.Data!.Replace(pattern, string.Empty);
    }
}
