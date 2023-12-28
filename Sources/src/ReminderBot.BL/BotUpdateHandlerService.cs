using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers;
using ReminderBot.Models.Enums;
using ReminderBot.Shared;
using ReminderBot.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ReminderBot.BL
{
    public class BotUpdateHandlerService : IBotUpdateHandlerService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBotHandlerManagerService _handlerManagerService;
        private readonly IBotContext _botContext;

        public BotUpdateHandlerService(IServiceProvider serviceProvider, IBotHandlerManagerService handlerManagerService, IBotContext botContext)
        {
            _serviceProvider = serviceProvider;
            _handlerManagerService = handlerManagerService;
            _botContext = botContext;
        }

        public async Task HandleMessageAsync(Update update, CancellationToken cancellationToken)
        {
            var userId = update.GetUserId();
            var command = update.GetCommand();
            var isCallbackCommand = update.Type == UpdateType.CallbackQuery;

            // To prevent Loading progress bar
            if (isCallbackCommand)
            {
                await _botContext.BotClient.AnswerCallbackQueryAsync(update.GetCallbackQueryId(), cancellationToken: cancellationToken);
            }

            var isStartNeeded = await GetIsStartNeeded(update);

            if (isStartNeeded && command != CommandConstants.TelegramBotCommandStart)
            {
                var unregHandler = ResolveHandler(CommandConstants.TelegramBotCommandUnregistered)!;

                await unregHandler.HandleUpdateAsync(update);

                return;
            }

            if (_handlerManagerService.TryGetHandler(userId, out var handler))
            {
                var isValid = IsValidForHandler(handler.State, update);
                var isIdleMode = handler.Command == CommandConstants.TelegramBotCommandIdle;
                var isIgnore = string.IsNullOrWhiteSpace(command)
                    || !command.StartsWith("/")
                    || (command.StartsWith("/") && handler.Command == command)
                    || !(handler.Command != command && isCallbackCommand);
                
                if (isValid && !isIdleMode && isIgnore)
                {
                    // do nothing
                }
                else
                {
                    if (!TryResolveCommandHandler(command, out handler))
                    {
                        handler = ResolveHandler();
                    }
                }
            }
            else if (!TryResolveCommandHandler(command, out handler))
            {
                handler = ResolveHandler();
            }

            try
            {
                bool handleNext;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    (handler, handleNext) = await handler!.HandleUpdateAsync(update);
                } while (handleNext);

                _handlerManagerService.AddOrUpdateHandler(userId, handler);
            }
            catch (OperationCanceledException) { /*Ignored*/ }
            catch
            {
                var exceptionHandler = ResolveHandler(CommandConstants.TelegramBotCommandException)!;
                (handler, _) = await exceptionHandler.HandleUpdateAsync(update);
                _handlerManagerService.AddOrUpdateHandler(userId, handler);

                throw;
            }
        }

        private async Task<bool> GetIsStartNeeded(Update update)
        {
            var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var userId = update.GetUserId();

            var isRegistered = await userService.IsRegisteredAsync(userId);

            return !(isRegistered && await userService.IsActive(userId));
        }

        private bool TryResolveCommandHandler(string? command, out BaseStateHandler? handler)
        {
            var handlerResolver = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<HandlerResolver>();

            handler = handlerResolver(command);

            return handler != null;
        }

        private BaseStateHandler ResolveHandler(string? command = null)
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<HandlerResolver>()(command);
        }

        private bool IsValidForHandler(BotHandlerStates state, Update update)
        {
            var updateType = update.Type;

            return state switch
            {
                BotHandlerStates.Idling => true,
                BotHandlerStates.Starting => true,
                BotHandlerStates.Waiting_Input_Message => updateType == UpdateType.Message,
                BotHandlerStates.Waiting_Input_Callback => updateType == UpdateType.CallbackQuery,
                _ => false
            };
        }
    }
}
