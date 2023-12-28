using System.Diagnostics.CodeAnalysis;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers;
using ReminderBot.Shared;

namespace ReminderBot.BL
{
    public class BotHandlerManagerService : IBotHandlerManagerService
    {
        private readonly IMemoryCacheManager _memoryCacheManager;
        private readonly IBotConfigurationService _telegramBotConfigurationService;

        public BotHandlerManagerService(IMemoryCacheManager memoryCacheManager, IBotConfigurationService telegramBotConfigurationService)
        {
            _memoryCacheManager = memoryCacheManager;
            _telegramBotConfigurationService = telegramBotConfigurationService;
        }

        public void AddOrUpdateHandler(long userId, BaseStateHandler handler)
        {
            _memoryCacheManager.AddOrReplace(GetCacheKey(userId), handler, _telegramBotConfigurationService.GetCacheLifetime());
        }

        public bool TryGetHandler(long userId, [NotNullWhen(true)] out BaseStateHandler handler)
        {
            return _memoryCacheManager.TryGet(GetCacheKey(userId), out handler!);
        }

        private string GetCacheKey(long userId)
        {
            return _memoryCacheManager.GetCacheKey(SystemConstants.TelegramBotUserHandler, userId);
        }
    }
}
