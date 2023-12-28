using System;
using ReminderBot.BL.Interfaces;
using Telegram.Bot;

namespace ReminderBot.BL
{
    public class BotContext : IBotContext
    {
        private ITelegramBotClient? _botClient;

        public ITelegramBotClient BotClient
        {
            get
            {
                if (_botClient != null)
                {
                    return _botClient;
                }

                throw new NullReferenceException("Failed to get TelegramBotClient");
            }
            set
            {
                _botClient = value;
            }
        }
    }
}
