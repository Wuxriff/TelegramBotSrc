using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ReminderBot.BL.Interfaces
{
    public interface IBotUpdateHandlerService
    {
        Task HandleMessageAsync(Update update, CancellationToken cancellationToken);
    }
}
