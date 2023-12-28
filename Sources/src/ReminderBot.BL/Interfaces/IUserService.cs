using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ReminderBot.Models.Models;

namespace ReminderBot.BL.Interfaces
{
    public interface IUserService
    {
        Task<bool> IsRegisteredAsync(long telegramUserId);
        Task AddOrUpdateAsync(TelegramUserModel user);
        [return: NotNull]
        Task<TelegramUserModel> GetAsync(long telegramUserId, bool useCached = true);
        Task DeleteAsync(long telegramUserId);
        Task<bool> IsActive(long telegramUserId);
    }
}
