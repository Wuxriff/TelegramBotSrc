using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReminderBot.BL;
using ReminderBot.BL.Interfaces;
using ReminderBot.DataAccess;

namespace ReminderBot.Tests
{
    public abstract class BaseTest
    {
        protected readonly IConfiguration Configuration;
        protected IServiceScopeFactory ServiceScopeFactory;

        protected bool InMemoryDatabase;

        protected BaseTest()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            InMemoryDatabase = Configuration.GetValue<bool>("UseInMemoryDb");

            var services = new ServiceCollection();
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();
            services.AddSingleton<IBotConfigurationService, BotConfigurationService>();
            services.AddSingleton<IBotContext, BotContext>();
            services.AddSingleton<IBotHandlerManagerService, BotHandlerManagerService>();
            services.AddScoped<IBotUpdateHandlerService, BotUpdateHandlerService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IHttpHelper, HttpHelper>();
            services.AddHttpClient();

            if (InMemoryDatabase)
            {
                services.AddDbContext<DataContext, DataContext>(options =>
                    options.UseInMemoryDatabase(nameof(DataContext))
                        .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            }
            else
            {
                //services.AddDbContext<DataContext, DataContext>(options =>
                //    options
                //        .UseSqlServer(Configuration.GetConnectionString(ConnectionStrings.TestDatabase))
                //        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                //    );
            }

            var serviceProvider = services.BuildServiceProvider();
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }
    }
}
