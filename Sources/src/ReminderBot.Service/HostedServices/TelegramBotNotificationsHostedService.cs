using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReminderBot.BL.Interfaces;

namespace ReminderBot.Service.HostedServices
{
    internal class TelegramBotNotificationsHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBotConfigurationService _botConfigurationService;
        private Task? _workerTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ILogger<TelegramBotNotificationsHostedService> _logger;

        public TelegramBotNotificationsHostedService(IServiceProvider serviceProvider, IBotConfigurationService botConfigurationService, ILogger<TelegramBotNotificationsHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _botConfigurationService = botConfigurationService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _workerTask = Task.Run(ProcessAsync, _cancellationTokenSource.Token);

                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                _logger.LogError("TelegramBotNotificationsHostedService StartAsync critical error: {0}", ex);
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping TelegramBotNotificationsHostedService");

                _cancellationTokenSource.Cancel();

                if (_workerTask != null)
                {
                    await _workerTask;
                }
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception ex)
            {
                _logger.LogError("TelegramBotNotificationsHostedService StopAsync critical error: {0}", ex);

            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task ProcessAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IReminderService>();
                        await service.SendAsync(_cancellationTokenSource.Token);
                    }

                    await Task.Delay(_botConfigurationService.GetCheckDelay(), _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Ignored
                }
                catch (Exception ex)
                {
                    _logger.LogError("TelegramBotNotificationsHostedService ProcessAsync critical error: {0}", ex);
                }
            }
        }
    }
}
