namespace ReminderBot.Shared
{
    public static class CommandConstants
    {
        public const string TelegramBotCommandStart = "/start";
        public const string TelegramBotCommandCancel = "/cancel";
        public const string TelegramBotCommandSettings = "/settings";
        public const string TelegramBotCommandHelp = "/help";
        public const string TelegramBotCommandStop = "/stop";
        public const string TelegramBotCommandUnregistered = "/unregistered";
        public const string TelegramBotCommandException = "/exception";
        public const string TelegramBotCommandIdle = "/idle";

        public const string TelegramBotCommandCreate = "/create";
        public const string TelegramBotCommandManage = "/manage";
        public const string TelegramBotCommandView = "/view";
        public const string TelegramBotCommandEdit = "/edit";
        public const string TelegramBotCommandPause = "/pause";
        public const string TelegramBotCommandResume = "/resume";
        public const string TelegramBotCommandReminder = "/reminder";

        public const string TelegramBotCommandDebug = "/debug";
        public const string TelegramBotCommandAdmin = "/admin";

        public const char CommandSeparator = '!';
    }
}
