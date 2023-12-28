using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReminderBot.BL.Interfaces;
using ReminderBot.DataAccess;
using ReminderBot.Mappings;
using ReminderBot.Models.Models;
using ReminderBot.Shared;
using Telegram.Bot.Types;

namespace ReminderBot.BL
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<DataContext> _contextFactory;
        private readonly IMemoryCacheManager _memoryCacheManager;
        private readonly IBotConfigurationService _telegramBotConfigurationService;

        public UserService(IDbContextFactory<DataContext> contextFactory, IMemoryCacheManager memoryCacheManager, IBotConfigurationService telegramBotConfigurationService)
        {
            _contextFactory = contextFactory;
            _memoryCacheManager = memoryCacheManager;
            _telegramBotConfigurationService = telegramBotConfigurationService;
        }

        public Task<bool> IsRegisteredAsync(long telegramUserId)
        {
            if (TryGetCache(telegramUserId, out _))
            {
                return Task.FromResult(true);
            }

            using var context = _contextFactory.CreateDbContext();

            var dbUser = context.TelegramUsers.AsNoTracking().Include(x => x.TelegramUserSettings).FirstOrDefault(x => x.UserId == telegramUserId);

            if (dbUser == null)
            {
                return Task.FromResult(false);
            }

            var user = TelegramUserModelMapper.MapTelegramUserModel(dbUser);

            AddOrReplaceCache(user);

            return Task.FromResult(true);
        }

        public Task AddOrUpdateAsync(TelegramUserModel user)
        {
            var dbUser = TelegramUserModelMapper.MapTelegramUser(user);

            using var context = _contextFactory.CreateDbContext();

            context.TelegramUsers.Update(dbUser);
            context.SaveChanges();

            var savedUser = TelegramUserModelMapper.MapTelegramUserModel(dbUser);

            AddOrReplaceCache(savedUser);

            return Task.CompletedTask;
        }

        public Task<TelegramUserModel> GetAsync(long telegramUserId, bool useCached = true)
        {
            if (useCached)
            {
                return Task.FromResult(GetOrAddCache(telegramUserId));
            }
            else
            {
                using var context = _contextFactory.CreateDbContext();

                var dbUser = context.TelegramUsers.AsNoTracking().Include(x => x.TelegramUserSettings).FirstOrDefault(x => x.UserId == telegramUserId);

                if (dbUser == null)
                {
                    throw new Exception($"User with TelegramUserId={telegramUserId} not found");
                }

                var user = TelegramUserModelMapper.MapTelegramUserModel(dbUser);

                AddOrReplaceCache(user);

                return Task.FromResult(user);
            }
        }

        public Task DeleteAsync(long telegramUserId)
        {
            using var context = _contextFactory.CreateDbContext();

            var dbUser = context.TelegramUsers.Include(x => x.TelegramUserSettings).Include(x => x.Reminders).FirstOrDefault(x => x.UserId == telegramUserId);

            if (dbUser == null)
            {
                throw new Exception($"User with TelegramUserId={telegramUserId} not found");
            }

            context.Remove(dbUser);
            context.SaveChanges();

            DeleteCache(telegramUserId);

            return Task.CompletedTask;
        }

        public Task<bool> IsActive(long telegramUserId)
        {
            var cachedUser = GetOrAddCache(telegramUserId);

            return Task.FromResult(cachedUser.IsActive);
        }

        #region Caching

        private string GetCacheKey(long telegramUserId)
        {
            return _memoryCacheManager.GetCacheKey(SystemConstants.TelegramBotUser, telegramUserId);
        }

        private bool TryGetCache(long telegramUserId, out TelegramUserModel? user)
        {
            return _memoryCacheManager.TryGet(GetCacheKey(telegramUserId), out user);
        }

        private void AddOrReplaceCache(TelegramUserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _memoryCacheManager.AddOrReplace(GetCacheKey(user.UserId), user, _telegramBotConfigurationService.GetCacheLifetime());
        }

        private TelegramUserModel GetOrAddCache(long telegramUserId)
        {
            return (_memoryCacheManager.GetOrAdd(GetCacheKey(telegramUserId),
                       () =>
                       {
                           using var context = _contextFactory.CreateDbContext();

                           var dbUser = context.TelegramUsers.AsNoTracking().Include(x => x.TelegramUserSettings).FirstOrDefault(x => x.UserId == telegramUserId);

                           if (dbUser == null)
                           {
                               throw new Exception($"User with TelegramUserId={telegramUserId} not found");
                           }

                           return TelegramUserModelMapper.MapTelegramUserModel(dbUser);
                       },
                        _telegramBotConfigurationService.GetCacheLifetime())
                   )!;
        }

        private void DeleteCache(long telegramUserId)
        {
            _memoryCacheManager.Remove(GetCacheKey(telegramUserId));
        }

        #endregion
    }
}
