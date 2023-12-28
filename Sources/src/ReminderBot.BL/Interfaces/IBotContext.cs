using Telegram.Bot;

namespace ReminderBot.BL.Interfaces
{
    public interface IBotContext
    {
        ITelegramBotClient BotClient { get; set; }
    }
}
