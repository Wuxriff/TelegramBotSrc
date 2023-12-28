using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReminderBot.Models.Models;

namespace ReminderBot.BL.Interfaces
{
    public interface IReminderService
    {
        Task AddAsync(long telegramUserId, string content, DateTime reminderDate);
        Task UpdateAsync(ReminderModel reminder);
        Task<ReminderModel> GetAsync(int reminderId);
        Task<int> GetUserRemindersCountAsync(long telegramUserId, bool includeConfirmed);
        Task<List<ReminderModel>> GetUserRemindersAsync(long telegramUserId, bool includeConfirmed, bool applyPagination = false, int skip = 0, int take = 0, bool orderByDesc = false);
        Task<List<ReminderModel>> GetListAsync(DateTime dateUtc, bool ignorePaused);
        Task DeleteAsync(int reminderId);
        Task MarkAsSentAsync(ReminderModel reminder);
        Task PostponeAsync(ReminderModel reminder, int postponeMinutes);
        Task ConfirmAsync(ReminderModel reminder);
        Task SendAsync(CancellationToken cancellationToken);
    }
}
