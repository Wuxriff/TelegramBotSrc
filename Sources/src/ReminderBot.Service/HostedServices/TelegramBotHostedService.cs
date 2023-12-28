using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReminderBot.BL.Interfaces;
using ReminderBot.DataAccess;

namespace ReminderBot.Service.HostedServices
{
    internal class TelegramBotHostedService : IHostedService, IDisposable
    {
        private readonly IBotService _telegramBotService;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ILogger<TelegramBotHostedService> _logger;

        public TelegramBotHostedService(IBotService telegramBotService, IServiceProvider serviceProvider, ILogger<TelegramBotHostedService> logger)
        {
            _telegramBotService = telegramBotService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var dataContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
                var dataContext = dataContextFactory.CreateDbContext();

                dataContext.Init();
                _telegramBotService.Init();

                await _telegramBotService.StartAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError("TelegramBotHostedService StartAsync critical error: {0}", ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping TelegramBotHostedService");

                _cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception ex)
            {
                _logger.LogError("TelegramBotHostedService StopAsync critical error: {0}", ex);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
