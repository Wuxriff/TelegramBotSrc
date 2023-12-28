using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReminderBot.BL.Interfaces;
using ReminderBot.Shared;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ReminderBot.BL
{
    public class BotService : IBotService
    {
        private readonly IBotConfigurationService _configurationService;
        private readonly IBotContext _botContext;
        private readonly IBotUpdateHandlerService _handlerService;
        private readonly ILocalizer _localizer;
        private readonly ILogger<BotService> _logger;

        private bool _isInitialized;

        public BotService(IBotConfigurationService configurationService, IBotContext botContext, IBotUpdateHandlerService handlerService, ILocalizer localizer, ILogger<BotService> logger)
        {
            _configurationService = configurationService;
            _botContext = botContext;
            _handlerService = handlerService;
            _localizer = localizer;
            _logger = logger;
        }

        public void Init()
        {
            if (_isInitialized) return;

            var apiToken = _configurationService.GetApiToken();

            _botContext.BotClient = new TelegramBotClient(apiToken);
            _isInitialized = true;
        }

        public async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Telegram bot in not initialized. Call Init() method first!");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            };

            _botContext.BotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            await DeleteRegisterdCommandsAsync(cancellationToken);
            await RegisterCommandsAsync(cancellationToken);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message or UpdateType.CallbackQuery => BotOnMessageReceived(botClient, update, cancellationToken),

                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await _handlerService.HandleMessageAsync(update, cancellationToken);
        }

        private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task DeleteRegisterdCommandsAsync(CancellationToken cancellationToken)
        {
            await _botContext.BotClient.DeleteMyCommandsAsync(languageCode: SystemConstants.EnLoc, cancellationToken: cancellationToken);
            await _botContext.BotClient.DeleteMyCommandsAsync(languageCode: SystemConstants.RuLoc, cancellationToken: cancellationToken);
            await _botContext.BotClient.DeleteMyCommandsAsync(languageCode: SystemConstants.BeLoc, cancellationToken: cancellationToken);
            await _botContext.BotClient.DeleteMyCommandsAsync(languageCode: SystemConstants.UkLoc, cancellationToken: cancellationToken);
        }

        private async Task RegisterCommandsAsync(CancellationToken cancellationToken)
        {
            foreach (var sysLoc in SystemConstants.AvailableLanguages.Keys)
            {
                var commands = new List<BotCommand>
                {
                    new BotCommand { Command = CommandConstants.TelegramBotCommandCancel, Description = _localizer.GetString(nameof(CommandConstants.TelegramBotCommandCancel), sysLoc), },
                    new BotCommand { Command = CommandConstants.TelegramBotCommandManage, Description = _localizer.GetString(nameof(CommandConstants.TelegramBotCommandManage), sysLoc), },
                    new BotCommand { Command = CommandConstants.TelegramBotCommandSettings, Description = _localizer.GetString(nameof(CommandConstants.TelegramBotCommandSettings), sysLoc), },
                    new BotCommand { Command = CommandConstants.TelegramBotCommandHelp, Description = _localizer.GetString(nameof(CommandConstants.TelegramBotCommandHelp), sysLoc), },
                    new BotCommand { Command = CommandConstants.TelegramBotCommandStop, Description = _localizer.GetString(nameof(CommandConstants.TelegramBotCommandStop), sysLoc), }
                };

                await _botContext.BotClient.SetMyCommandsAsync(commands, languageCode: sysLoc, cancellationToken: cancellationToken);
            }
        }
    }
}
