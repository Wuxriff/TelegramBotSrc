using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL;
using ReminderBot.BL.Interfaces;
using ReminderBot.BL.StateHandlers.Reminders;
using ReminderBot.BL.StateHandlers.System;
using ReminderBot.Service.HostedServices;
using ReminderBot.Shared;

namespace ReminderBot.Service.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<TelegramBotHostedService>();
            services.AddHostedService<TelegramBotNotificationsHostedService>();
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();
            services.AddSingleton<IBotConfigurationService, BotConfigurationService>();
            services.AddSingleton<IBotContext, BotContext>();
            services.AddSingleton<IBotHandlerManagerService, BotHandlerManagerService>();
            services.AddSingleton<ILocalizer, Localizer>();

            services.AddScoped<IBotUpdateHandlerService, BotUpdateHandlerService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IHttpHelper, HttpHelper>();
            services.AddScoped<IReminderService, ReminderService>();
            services.AddScoped<IUserService, UserService>();

            services.AddHttpClient();
        }

        public static void AddOption<T>(this IServiceCollection services, IConfiguration configuration) where T : class
        {
            services.Configure<T>(configuration.GetSection(typeof(T).Name));
        }

        public static void AddHandlers(this IServiceCollection services)
        {
            services.AddTransient<UnknownHandler>();
            services.AddTransient<UnregisteredHandler>();
            services.AddTransient<StartHandler>();
            services.AddTransient<CancelHandler>();
            services.AddTransient<SettingsHandler>();
            services.AddTransient<HelpHandler>();
            services.AddTransient<StopHandler>();
            services.AddTransient<CreateHandler>();
            //services.AddTransient<ViewHandler>();
            services.AddTransient<EditHandler>();
            services.AddTransient<PauseHandler>();
            services.AddTransient<ResumeHandler>();
            services.AddTransient<ReminderHandler>();
            services.AddTransient<ManageHandler>();
            services.AddTransient<DebugHandler>();
            services.AddTransient<AdminHandler>();
            services.AddTransient<ExceptionHandler>();
        }

        public static void AddCommands(this IServiceCollection services)
        {
            services.AddTransient<HandlerResolver>(serviceProvider => command =>
            {
                return command switch
                {
                    CommandConstants.TelegramBotCommandStart => serviceProvider.GetRequiredService<StartHandler>(),
                    CommandConstants.TelegramBotCommandCancel => serviceProvider.GetRequiredService<CancelHandler>(),
                    CommandConstants.TelegramBotCommandSettings => serviceProvider.GetRequiredService<SettingsHandler>(),
                    CommandConstants.TelegramBotCommandHelp => serviceProvider.GetRequiredService<HelpHandler>(),
                    CommandConstants.TelegramBotCommandStop => serviceProvider.GetRequiredService<StopHandler>(),
                    CommandConstants.TelegramBotCommandDebug => serviceProvider.GetRequiredService<DebugHandler>(),
                    CommandConstants.TelegramBotCommandUnregistered => serviceProvider.GetRequiredService<UnregisteredHandler>(),

                    CommandConstants.TelegramBotCommandCreate => serviceProvider.GetRequiredService<CreateHandler>(),
                    //CommandConstants.TelegramBotCommandView => serviceProvider.GetRequiredService<ViewHandler>(),
                    CommandConstants.TelegramBotCommandEdit => serviceProvider.GetRequiredService<EditHandler>(),
                    CommandConstants.TelegramBotCommandPause => serviceProvider.GetRequiredService<PauseHandler>(),
                    CommandConstants.TelegramBotCommandResume => serviceProvider.GetRequiredService<ResumeHandler>(),
                    CommandConstants.TelegramBotCommandReminder => serviceProvider.GetRequiredService<ReminderHandler>(),
                    CommandConstants.TelegramBotCommandManage => serviceProvider.GetRequiredService<ManageHandler>(),
                    CommandConstants.TelegramBotCommandAdmin => serviceProvider.GetRequiredService<AdminHandler>(),
                    CommandConstants.TelegramBotCommandException => serviceProvider.GetRequiredService<ExceptionHandler>(),

                    _ => serviceProvider.GetRequiredService<UnknownHandler>()
                };
            });
        }
    }
}
