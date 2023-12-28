using ReminderBot.Models.Enums;
using ReminderBot.Shared;

namespace ReminderBot.BL.Helpers
{
    public static class CommandHelper
    {
        public static string MakeCommandArgs(string Command, BotCallbackArgs state, params object[] args)
        {
            return $"{Command}{CommandConstants.CommandSeparator}{state}{(args?.Length == 0
                ? string.Empty
                : CommandConstants.CommandSeparator + string.Join(CommandConstants.CommandSeparator, args!))}";
        }
    }
}
