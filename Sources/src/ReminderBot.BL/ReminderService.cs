using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReminderBot.BL.Helpers;
using ReminderBot.BL.Interfaces;
using ReminderBot.DataAccess;
using ReminderBot.Entities;
using ReminderBot.Mappings;
using ReminderBot.Models.Enums;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReminderBot.BL
{
    public class ReminderService : IReminderService
    {
        private readonly IDbContextFactory<DataContext> _contextFactory;
        private readonly IUserService _userService;
        private readonly IBotContext _botContext;
        private readonly IBotConfigurationService _botConfigurationService;
        private readonly ILocalizer _localizer;

        private const string ReminderWrapper = "\U000023F0 {0}";

        public ReminderService(IDbContextFactory<DataContext> contextFactory, IUserService userService, IBotContext botContext,
            IBotConfigurationService botConfigurationService, ILocalizer localizer)
        {
            _contextFactory = contextFactory;
            _userService = userService;
            _botContext = botContext;
            _botConfigurationService = botConfigurationService;
            _localizer = localizer;
        }

        public async Task AddAsync(long telegramUserId, string content, DateTime reminderDate)
        {
            var user = await _userService.GetAsync(telegramUserId, true);

            if (user == null)
            {
                throw new Exception($"User with TelegramUserId={telegramUserId} not found");
            }

            var zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TelegramUserSettings.TimeZoneId!);
            var reminderDateUtc = TimeZoneInfo.ConvertTimeToUtc(reminderDate, zoneInfo);

            var reminder = new ReminderModel
            {
                TelegramUser = user,
                TelegramUserId = user.Id,
                Base64Content = content.ToBase64(),
                CreatedDateUtc = DateTime.UtcNow,
                ReminderDate = reminderDate,
                OriginalReminderDate = reminderDate,
                ReminderDateUtc = reminderDateUtc,
                OriginalReminderDateUtc = reminderDateUtc,
            };

            var dbReminder = ReminderModelMapper.MapReminder(reminder);

            using var context = _contextFactory.CreateDbContext();

            context.Reminders.Update(dbReminder);
            context.SaveChanges();
        }

        public Task UpdateAsync(ReminderModel reminder)
        {
            if (reminder == null)
            {
                throw new Exception("Reminder cant be null");
            }

            var dbReminder = ReminderModelMapper.MapReminder(reminder);

            using var context = _contextFactory.CreateDbContext();

            context.Reminders.Update(dbReminder);
            context.SaveChanges();

            return Task.CompletedTask;
        }

        public Task<ReminderModel> GetAsync(int reminderId)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbReminder = context.Reminders.Include(x => x.TelegramUser).Include(x => x.TelegramUser.TelegramUserSettings)
                .AsNoTracking().FirstOrDefault(x => x.Id == reminderId);

            if (dbReminder == null)
            {
                throw new Exception($"Reminder with reminderId={reminderId} not found");
            }

            var mappedModel = ReminderModelMapper.MapReminderModel(dbReminder);

            mappedModel.Content = mappedModel.Base64Content.FromBase64();

            return Task.FromResult(mappedModel);
        }

        public Task<int> GetUserRemindersCountAsync(long telegramUserId, bool includeConfirmed)
        {
            using var context = _contextFactory.CreateDbContext();

            IQueryable<Reminder> query = context.Reminders.AsNoTracking().Include(x => x.TelegramUser).Include(x => x.TelegramUser.TelegramUserSettings).Where(x => x.TelegramUser.UserId == telegramUserId);

            if (includeConfirmed == false)
            {
                query = query.Where(x => x.IsConfirmed == false);
            }

            return Task.FromResult(query.Count());
        }

        public Task<List<ReminderModel>> GetUserRemindersAsync(long telegramUserId, bool includeConfirmed, bool applyPagination = false, int skip = 0, int take = 0, bool orderByDesc = false)
        {
            using var context = _contextFactory.CreateDbContext();

            IQueryable<Reminder> query = context.Reminders.AsNoTracking().Include(x => x.TelegramUser).Include(x => x.TelegramUser.TelegramUserSettings).Where(x => x.TelegramUser.UserId == telegramUserId);

            if (includeConfirmed == false)
            {
                query = query.Where(x => x.IsConfirmed == false);
            }

            if (orderByDesc)
            {
                query = query.OrderByDescending(x => x.ReminderDate);
            }

            if (applyPagination)
            {
                if (skip > 0)
                {
                    query = query.Skip(skip);
                }

                if (take > 0)
                {
                    query = query.Take(take);
                }
            }

            var dbReminders = query.ToArray();

            var result = new List<ReminderModel>(dbReminders.Length);

            foreach (var dbReminder in dbReminders)
            {
                var reminder = ReminderModelMapper.MapReminderModel(dbReminder);
                reminder.Content = reminder.Base64Content.FromBase64();

                result.Add(reminder);
            }

            return Task.FromResult(result);
        }

        public Task<List<ReminderModel>> GetListAsync(DateTime dateUtc, bool ignorePaused)
        {
            var offsetTime = DateTime.UtcNow.AddMinutes(-_botConfigurationService.GetConfirmDelayMinutes());

            using var context = _contextFactory.CreateDbContext();

            var dbReminders = context.Reminders.Include(x => x.TelegramUser).Include(x => x.TelegramUser.TelegramUserSettings)
                .Where(x => x.IsConfirmed == false
                && (x.DateSentUtc == null || x.DateSentUtc <= offsetTime)
                && x.ReminderDateUtc <= dateUtc
                && (!ignorePaused || !x.TelegramUser.TelegramUserSettings.IsPaused))
                .AsNoTracking().ToArray();

            var result = new List<ReminderModel>(dbReminders.Length);

            foreach (var dbReminder in dbReminders)
            {
                var reminder = ReminderModelMapper.MapReminderModel(dbReminder);
                reminder.Content = reminder.Base64Content.FromBase64();

                result.Add(reminder);
            }

            return Task.FromResult(result);
        }

        public Task DeleteAsync(int reminderId)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbReminder = context.Reminders.FirstOrDefault(x => x.Id == reminderId);

            if (dbReminder == null)
            {
                throw new Exception($"Reminder with reminderId={reminderId} not found");
            }

            context.Remove(dbReminder);
            context.SaveChanges();

            return Task.CompletedTask;
        }

        public Task MarkAsSentAsync(ReminderModel reminder)
        {
            if (reminder == null)
            {
                throw new Exception("Reminder cant be null");
            }

            var date = DateTime.UtcNow;

            reminder.DateSentUtc = date;

            if (reminder.OriginalDateSentUtc == null)
            {
                reminder.OriginalDateSentUtc = date;
            }

            return UpdateAsync(reminder);
        }

        public Task PostponeAsync(ReminderModel reminder, int postponeMinutes)
        {
            if (reminder == null)
            {
                throw new Exception("Reminder cant be null");
            }

            var nextDate = DateTime.Now.AddMinutes(postponeMinutes);

            reminder.ReminderDate = nextDate;
            reminder.ReminderDateUtc = nextDate.ToUniversalTime();
            reminder.DateSentUtc = null;

            return UpdateAsync(reminder);
        }

        public Task ConfirmAsync(ReminderModel reminder)
        {
            if (reminder == null)
            {
                throw new Exception("Reminder cant be null");
            }

            reminder.IsConfirmed = true;

            return UpdateAsync(reminder);
        }

        public async Task SendAsync(CancellationToken cancellationToken)
        {
            var reminders = await GetListAsync(DateTime.UtcNow, true);

            foreach (var reminder in reminders)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var postponeMinutes = reminder.TelegramUser.TelegramUserSettings.PostponeMinutes;
                var languageCode = reminder.TelegramUser.LanguageCode;

                var markup = GetReminerdMarkup(postponeMinutes, reminder.Id, languageCode);
                var message = string.Format(ReminderWrapper, reminder.Content);
                var chatId = reminder.TelegramUser.ChatId;

                if (reminder.MessageId != null)
                {
                    try
                    {
                        await _botContext.BotClient.DeleteMessageAsync(chatId, reminder.MessageId.Value, cancellationToken: cancellationToken);
                    }
                    catch (ApiRequestException)
                    {
                        // Could be deleted by the user

                        reminder.MessageId = null;
                        await UpdateAsync(reminder);
                    }
                }

                reminder.MessageId = (await _botContext.BotClient.SendTextMessageAsync(chatId, message, replyMarkup: markup, cancellationToken: cancellationToken)).MessageId;

                await MarkAsSentAsync(reminder);
            }
        }

        private InlineKeyboardMarkup GetReminerdMarkup(int postponeMinutes, int reminderId, string languageCode)
        {
            var postponeCallback = CommandHelper.MakeCommandArgs(CommandConstants.TelegramBotCommandReminder, BotCallbackArgs.ReminderHandler_Postpone, reminderId);
            var confirmCallback = CommandHelper.MakeCommandArgs(CommandConstants.TelegramBotCommandReminder, BotCallbackArgs.ReminderHandler_Confirm, reminderId);

            return new InlineKeyboardMarkup(
              new[]
              {
                    InlineKeyboardButton.WithCallbackData(_localizer.GetString("Reminder_PostponeFormatButton", languageCode, postponeMinutes), postponeCallback),
                    InlineKeyboardButton.WithCallbackData(_localizer.GetString("Reminder_ConfirmButton", languageCode), confirmCallback),
              });
        }
    }
}
